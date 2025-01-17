﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Principal;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Linq.Expressions;
using System.Xml.Linq;

using System.Text.RegularExpressions;

namespace WinContextTweaker
{
	public partial class MainWindow : Window
	{
		public string? path;
		public string? selectedScript;

		public MainWindow()
		{
			if (IsElevated())
			{
				InitializeComponent();

				CanEdit(false);
				CanSee(false);
				cmbContext.SelectedIndex = 0;
			}
			else
			{
				MessageBox.Show("You need to run this program with Elevated Permissions to edit the Registry!");
				this.Close();
			}
		}


		static private bool IsElevated()
		{
			bool isElevated;
			using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
			{
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
			}

			return isElevated;
		}



		private void UpdateOptions()
		{
			RegistryKey? script = OpenRootSubKey(path + "shell\\" + selectedScript);

			if (script != null)
			{
				foreach (CheckBox checkbox in stkOptions.Children)
				{
					checkbox.IsChecked = script.GetValue(checkbox.DataContext as string) != null;
				}
			}
		}
		private void UpdateScripts()
		{
			lstScripts.Items.Clear();

			if (path != null)
			{
				string[] subKeyNames = OpenRootSubKey(path + "shell\\")?.GetSubKeyNames() ?? [];

				//Debug.WriteLine(string.Join(" ", subKeyNames));

				foreach (string name in subKeyNames)
				{
					if (GetRootScriptCommand(path + "shell\\" + name) != null)
					{
						lstScripts.Items.Add(name);
					}
				}

				lblNoScriptFound.Visibility = lstScripts.HasItems ? Visibility.Collapsed : Visibility.Visible;
			}
		}
		private void CanSee(bool allow)
		{
			Visibility visibility = allow ? Visibility.Visible : Visibility.Collapsed;

			stkCommand.Visibility = visibility;
			stkOptions.Visibility = visibility;
		}
		private void CanEdit(bool allow)
		{
			txtCommand.IsEnabled = allow;
			stkOptions.IsEnabled = allow;
			btnRenameScript.IsEnabled = allow;
			btnDeleteScript.IsEnabled = allow;
		}



		static private RegistryKey? OpenRootSubKey(string name, bool writable = false)
		{
			return Registry.ClassesRoot.OpenSubKey(name, writable);
		}
		static private RegistryKey? CreateRootSubKey(string path, string key)
		{
			return OpenRootSubKey(path, true)?.CreateSubKey(key);
		}
		static private void DeleteRootSubKeyTree(string path, string key)
		{
			OpenRootSubKey(path, true)?.DeleteSubKeyTree(key);
		}


		static private string? GetRootScriptCommand(string path)
		{
			RegistryKey? key = OpenRootSubKey(path + "\\command");

			if (key == null) return null;

			return key.GetValue("") as string ?? "";
		}
		static private void SetRootScriptCommand(string path, string command)
		{
			OpenRootSubKey(path + "\\command", true)?.SetValue("", command);
		}
		static private void RenameRootScript(string path, string oldKey, string newKey)
		{
			RegistryKey? oldScript = OpenRootSubKey(path + oldKey);

			if (oldScript != null)
			{
				RegistryKey? newScript = CreateRootSubKey(path, newKey);
				CreateRootSubKey(path, newKey + "\\command");

				if (newScript != null)
				{
					string command = GetRootScriptCommand(path + oldKey) ?? "";
					SetRootScriptCommand(path + newKey, command);

					string[] enabledOptions = oldScript.GetValueNames();
					foreach (string name in enabledOptions)
					{
						newScript.SetValue(name, "");
					}

					DeleteRootSubKeyTree(path, oldKey);
				}
			}
		}
		static private void SwitchScriptOption(string path, string option, bool enable, string value = "")
		{
			RegistryKey? script = OpenRootSubKey(path, true);

			if (script != null)
			{
				if (enable)
				{
					script.SetValue(option, value);
				}
				else if (script.GetValue(option) != null)
				{
					script.DeleteValue(option);
				}
			}
		}


		static private bool IsValidRegexKeyName(string str)
		{
			const string invalidChars = """\""";

			Regex InvalidCharsRegex = new Regex($"[{Regex.Escape(invalidChars)}]");

			if (InvalidCharsRegex.IsMatch(str) || str.Length == 0) return false;
			return true;
		}



		private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.F5:
					UpdateScripts();
					UpdateOptions();
					break;
			}
		}

		private void Context_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CanEdit(false);

			ComboBoxItem? selectedItem = e.AddedItems[0] as ComboBoxItem;

			if (selectedItem == null) return;

			path = selectedItem.DataContext as string;

			if (path == "SystemFileAssociations\\")
			{
				lstScripts.Items.Clear();

				InputDialog inputDialog = new InputDialog("Enter the file extension (WITH A POINT)", ".");

				if (inputDialog.ShowDialog() == true)
				{
					path += inputDialog.Answer + "\\";
					lblSelectedExtension.Content = inputDialog.Answer;

					CreateRootSubKey("SystemFileAssociations", inputDialog.Answer);

					UpdateScripts();

					lblSelectedExtension.Content = inputDialog.Answer;
					lblSelectedExtension.Visibility = Visibility.Visible;
				}
				else
				{
					cmbContext.SelectedIndex = 0;
				}
			}
			else
			{
				lblSelectedExtension.Visibility = Visibility.Collapsed;

				UpdateScripts();
			}
		}

		private void List_ScriptChanged(object sender, EventArgs e)
		{
			selectedScript = lstScripts.SelectedItem as string;

			if (selectedScript == null)
			{
				CanEdit(false);
				CanSee(false);
			}
			else
			{
				CanSee(true);

				try
				{
					UpdateOptions();

					string? command = GetRootScriptCommand(path + "shell\\" + selectedScript);
					txtCommand.Text = command;

					CanEdit(true);
				}
				catch
				{
					CanEdit(false);
				}
			}
		}

		private void Checkbox_SwitchScriptOption(object sender, EventArgs e)
		{
			CheckBox element = (CheckBox)sender;
			bool state = element.IsChecked ?? false;
			string? option = element.DataContext as string;

			if (option == null) return;

			string value = "";
			if (option == "MultiSelectModel") value = "player";

			SwitchScriptOption(path + "shell\\" + selectedScript, option, state, value);

			if (option == "Icon" && state == true)
			{
				element.IsChecked = false;

				OpenFileDialog openFileDialog = new OpenFileDialog();

				openFileDialog.InitialDirectory = "%userprofile%";
				openFileDialog.Filter = "Icon Files (*.ico, *.exe)|*.ico;*.exe";
				openFileDialog.FilterIndex = 0;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == true)
				{
					string iconPath = openFileDialog.FileName;

					OpenRootSubKey(path + "shell\\" + selectedScript, true)?.SetValue(option, iconPath);

					element.IsChecked = true;
				}
			}
		}

		private void Text_CommandChanged(object sender, EventArgs e)
		{
			string command = txtCommand.Text;

			SetRootScriptCommand(path + "shell\\" + selectedScript, command);
		}



		private void Button_NewScript(object sender, EventArgs e)
		{
			const string defaultCommand = "cmd /c \"echo Hello World && pause\"";

			InputDialog inputDialog = new InputDialog("Enter the new script's name:", "");

			if (inputDialog.ShowDialog() == true && path != null)
			{
				if (IsValidRegexKeyName(inputDialog.Answer))
				{
					CreateRootSubKey(path, $"shell\\{inputDialog.Answer}\\command");
					SetRootScriptCommand(path + "shell\\" + inputDialog.Answer, defaultCommand);
					UpdateScripts();
				}
			}
		}

		private void Button_DeleteScript(object sender, EventArgs e)
		{
			Prompt prompt = new Prompt("Are you sure you want to delete this script?");

			if (prompt.ShowDialog() == true && path != null && selectedScript != null)
			{
				DeleteRootSubKeyTree(path + "shell\\", selectedScript);
				UpdateScripts();
			}
		}

		private void Button_RenameScript(object sender, EventArgs e)
		{
			if (selectedScript == null) return;

			InputDialog inputDialog = new InputDialog("Enter the script's new name:", selectedScript);

			if (inputDialog.ShowDialog() == true && path != null && selectedScript != null)
			{
				if (IsValidRegexKeyName(inputDialog.Answer))
				{
					RenameRootScript(path + "shell\\", selectedScript, inputDialog.Answer);
					UpdateScripts();
				}
			}
		}
	}
}