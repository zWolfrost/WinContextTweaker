using System;
using System.Windows;
using System.Windows.Controls;

namespace WinContextTweaker
{
	public partial class InputDialog : Window
	{
		public InputDialog(string question, string defaultAnswer = "")
		{
			InitializeComponent();
			lblQuestion.Content = question;
			txtAnswer.Text = defaultAnswer;
		}

		private void Button_DialogOkClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			txtAnswer.Focus();
			txtAnswer.SelectionStart = txtAnswer.Text.Length;
			txtAnswer.SelectionLength = 0;
		}

		public string Answer
		{
			get { return txtAnswer.Text; }
		}
	}
}
