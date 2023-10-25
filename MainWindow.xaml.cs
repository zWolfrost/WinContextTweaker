using System;
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
			RegistryKey? script = OpenRootSubKey(path + selectedScript);

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
				string[] subKeyNames = OpenRootSubKey(path)?.GetSubKeyNames() ?? [];

				/*Debug.WriteLine(string.Join(" ", subKeyNames));*/

				foreach (string name in subKeyNames)
				{
					if (GetRootScriptCommand(path + name) != null)
					{
						lstScripts.Items.Add(name);
					}
				}
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

			if (oldScript != null && newKey != "")
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
		static private void SwitchScriptOption(string path, string option, bool enable)
		{
			RegistryKey? script = OpenRootSubKey(path, true);

			if (script != null)
			{
				if (enable)
				{
					script.SetValue(option, "");
				}
				else if (script.GetValue(option) != null)
				{
					script.DeleteValue(option);
				}
			}
		}



		private void Context_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CanEdit(false);

			path = ((ComboBoxItem)e.AddedItems[0]).DataContext.ToString();

			if (path == "SystemFileAssociations\\")
			{
				path = null;
				btnSelectExtension.Content = "Select Extension";
				btnSelectExtension.Visibility = Visibility.Visible;
			}
			else
			{
				btnSelectExtension.Visibility = Visibility.Collapsed;
			}

			UpdateScripts();
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

					string? command = GetRootScriptCommand(path + selectedScript);
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

			if (option != null)
			{
				SwitchScriptOption(path + selectedScript, option, state);

				if (state == true)
				{
					switch (option) //special option scripts
					{
						case "Icon":
							OpenFileDialog openFileDialog = new OpenFileDialog();

							openFileDialog.InitialDirectory = "%userprofile%";
							openFileDialog.Filter = "Icon Files (*.ico, *.exe)|*.ico;*.exe";
							openFileDialog.FilterIndex = 0;
							openFileDialog.RestoreDirectory = true;

							if (openFileDialog.ShowDialog() == true)
							{
								string iconPath = openFileDialog.FileName;

								OpenRootSubKey(path + selectedScript, true)?.SetValue(option, iconPath);
							}
							else
							{
								element.IsChecked = false;
							}

							break;
					}
				}
			}
		}

		private void Text_CommandChanged(object sender, EventArgs e)
		{
            string command = txtCommand.Text;
			
			SetRootScriptCommand(path + selectedScript, command);
		}



		private void Button_NewScript(object sender, EventArgs e)
		{
			const string defaultCommand = "cmd /c \"echo Hello World && pause\"";

			InputDialog inputDialog = new InputDialog("Enter the new script's name:", "");

			if (inputDialog.ShowDialog() == true && path != null)
			{
				CreateRootSubKey(path, inputDialog.Answer + "\\command");
				SetRootScriptCommand(path + inputDialog.Answer, defaultCommand);
				UpdateScripts();
			}
		}

		private void Button_DeleteScript(object sender, EventArgs e)
		{
			Prompt prompt = new Prompt("Are you sure you want to delete this script?");

			if (prompt.ShowDialog() == true)
			{
				DeleteRootSubKeyTree(path, selectedScript);
				UpdateScripts();
			}
		}

		private void Button_RenameScript(object sender, EventArgs e)
		{
			InputDialog inputDialog = new InputDialog("Enter the script's new name:", selectedScript);

			if (inputDialog.ShowDialog() == true)
			{
				RenameRootScript(path, selectedScript, inputDialog.Answer);
				UpdateScripts();
			}
		}

		private void Button_SelectExtension(object sender, EventArgs e)
		{
			InputDialog inputDialog = new InputDialog("Enter the file extension (WITH A POINT)", ".");

			if (inputDialog.ShowDialog() == true)
			{
				path = $"SystemFileAssociations\\{inputDialog.Answer}\\shell\\";
				btnSelectExtension.Content = inputDialog.Answer;
			}

			UpdateScripts();
		}
	}
}