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
				string[] enabledOptions = script.GetValueNames();

				Extended.IsChecked = enabledOptions.Contains("Extended");
			}
			else
			{
				Extended.IsChecked = false;
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
					lstScripts.Items.Add(name);
				}
			}
		}
		private void CanEdit(bool allow)
		{
			txtCommand.IsEnabled = allow;
			stkOptions.IsEnabled = allow;
			btnEditScript.IsEnabled = allow;
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
			return OpenRootSubKey(path + "\\command")?.GetValue("") as string;
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



		private void Context_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CanEdit(false);

			path = ((ComboBoxItem)e.AddedItems[0]).DataContext.ToString();

			UpdateScripts();
        }

        private void List_ScriptChanged(object sender, EventArgs e)
		{
			selectedScript = lstScripts.SelectedItem as string;

			if (selectedScript == null)
			{
				CanEdit(false);
				txtCommand.Text = "";
			}
            else
            {
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
			string option = element.Name;

			RegistryKey? script = OpenRootSubKey(path + selectedScript, true);

			if (script != null)
			{
				if (state)
				{
					script.SetValue(option, "");
				}
				else if (script.GetValue(option) != null)
				{
					script.DeleteValue(option);
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
			InputDialog inputDialog = new InputDialog("Enter the new script's name:", "");

			if (inputDialog.ShowDialog() == true)
			{
				CreateRootSubKey(path, inputDialog.Answer + "\\command");
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

		private void Button_EditScript(object sender, EventArgs e)
		{
			InputDialog inputDialog = new InputDialog("Enter the script's new name:", selectedScript);

			if (inputDialog.ShowDialog() == true)
			{
				RenameRootScript(path, selectedScript, inputDialog.Answer);
				UpdateScripts();
			}
		}
	}
}