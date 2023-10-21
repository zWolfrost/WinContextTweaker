using System;
using System.Windows;

namespace WinContextTweaker
{
	public partial class Prompt : Window
	{
		public Prompt(string question)
		{
			InitializeComponent();
			lblQuestion.Content = question;
		}

		private void Button_DialogYesClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
