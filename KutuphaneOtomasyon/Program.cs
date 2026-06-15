namespace KutuphaneOtomasyon;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        DatabaseMigrator.Initialize();
        if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        while (true)
        {
            using var loginForm = new LoginForm();
            if (loginForm.ShowDialog() != DialogResult.OK || loginForm.LoggedInUser is null)
            {
                break;
            }

            using var mainForm = new MainForm(loginForm.LoggedInUser);
            Application.Run(mainForm);

            if (!mainForm.LogoutRequested)
            {
                break;
            }
        }
    }    
}
