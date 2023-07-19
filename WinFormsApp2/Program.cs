namespace WinFormsApp2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Model m = new Model(); // creating a model instance
            Form1 v = new Form1(); // creating a form instance
            Computer_Player cp = new Computer_Player(m); // creating a Computer Player instance (can be considered as a 2nd part of the model)
            Presenter p = new Presenter(v, m, cp); // creating a Presenter instance

            Application.Run(v);
        }

        private static void F_player_plays_zero(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void F_difficulty_medium(Form f, int num = 0)
        {
            throw new NotImplementedException();
        }
    }
}