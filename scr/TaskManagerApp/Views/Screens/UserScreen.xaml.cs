using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TaskManager.Views.Screens
{
    /// <summary>
    /// Represents the screen for managing users within the application.
    /// Provides a user interface to view, add, edit, or remove users.
    /// </summary>
    public partial class UsersScreen : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersScreen"/> class.
        /// Sets up the user interface components for user management functionalities.
        /// </summary>
        public UsersScreen()
        {
            InitializeComponent();
        }
    }
}
