using System.Windows.Controls;
using TaskManager.Services;

namespace TaskManager.Views.Screens
{
    /// <summary>
    /// Interaction logic for NewUserScreen.xaml
    /// This screen allows admin users to create a new user and save it to the database.
    /// </summary>
    public partial class NewUserScreen : UserControl
    {
        /// <summary>
        /// Constructor initializes the UI components of the NewUserScreen.
        /// </summary>
        public NewUserScreen()
        {
            InitializeComponent();
        }
    }
}
