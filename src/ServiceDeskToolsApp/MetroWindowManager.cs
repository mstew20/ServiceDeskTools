using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ServiceDeskToolsApp.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ControlzEx.Theming;

namespace ServiceDeskToolsApp
{
    public class MetroWindowManager : IWindowManager
    {
        /// <summary>
        /// Shows a modal dialog for the specified model.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The dialog popup settings.</param>
        /// <returns>The dialog result.</returns>
        public virtual async Task<bool?> ShowDialogAsync(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            var window = await CreateWindowAsync(rootModel, true, context, settings);

            return window.ShowDialog();
        }

        public Task ShowPopupAsync(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shows a window for the specified model.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional window settings.</param>
        public virtual async Task ShowWindowAsync(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {

            NavigationWindow navWindow = null;

            var application = Application.Current;
            if (application != null && application.MainWindow != null)
            {
                navWindow = application.MainWindow as NavigationWindow;
            }

            if (navWindow != null)
            {
                //var window = await CreatePageAsync(rootModel, context, settings);
                //navWindow.Navigate(window);
            }
            else
            {
                var window = await CreateWindowAsync(rootModel, false, context, settings);
                window.Show();
            }
        }


        /// <summary>
        /// Creates a window.
        /// </summary>
        /// <param name="rootModel">The view model.</param>
        /// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
        /// <param name="context">The view context.</param>
        /// <param name="settings">The optional popup settings.</param>
        /// <returns>The window.</returns>
        protected virtual async Task<MetroWindow> CreateWindowAsync(object rootModel, bool isDialog, object context, IDictionary<string, object> settings)
        {
            var view = EnsureWindow(rootModel, ViewLocator.LocateForModel(rootModel, null, context), isDialog);
            ViewModelBinder.Bind(rootModel, view, context);

            var haveDisplayName = rootModel as IHaveDisplayName;
            if (string.IsNullOrEmpty(view.Title) && haveDisplayName != null && !ConventionManager.HasBinding(view, Window.TitleProperty))
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
                view.SetBinding(Window.TitleProperty, binding);
            }

            ApplySettings(view, settings);

            view.TitleCharacterCasing = CharacterCasing.Normal;
            var uri = new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\Images\\logo.ico");
            view.Icon = new BitmapImage(uri);


            var conductor = new WindowConductor(rootModel, view);

            await conductor.InitialiseAsync();
            return view;
        }

        /// <summary>
        /// Makes sure the view is a window is is wrapped by one.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <param name="view">The view.</param>
        /// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
        /// <returns>The window.</returns>
        protected virtual MetroWindow EnsureWindow(object model, object view, bool isDialog)
        {

            if (view is MetroWindow window)
            {
                var owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                {
                    window.Owner = owner;
                }
            }
            else
            {
                window = new MetroWindow
                {
                    Content = view,
                    SizeToContent = SizeToContent.WidthAndHeight,
                };

                window.SetValue(View.IsGeneratedProperty, true);

                var owner = InferOwnerOf(window);
                if (owner != null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }

            //window.SetResourceReference(MetroWindow.WindowTitleBrushProperty, "MahApps.Brushes.Accent2");
            //window.SetResourceReference(MetroWindow.NonActiveBorderBrushProperty, "MahApps.Brushes.Accent2");
            //window.SetResourceReference(MetroWindow.NonActiveWindowTitleBrushProperty, "MahApps.Brushes.Accent2");
            //window.SetResourceReference(MetroWindow.NonActiveBorderBrushProperty, "MahApps.Brushes.Accent2");
            //window.SetResourceReference(MetroWindow.NonActiveGlowBrushProperty, "MahApps.Brushes.Accent2");
            window.SetResourceReference(MetroWindow.GlowBrushProperty, "MahApps.Brushes.Accent2");
            //window.SetResourceReference(MetroWindow.BorderBrushProperty, "MahApps.Brushes.Accent2");

            return window;
        }

        /// <summary>
        /// Infers the owner of the window.
        /// </summary>
        /// <param name="window">The window to whose owner needs to be determined.</param>
        /// <returns>The owner.</returns>
        protected virtual MetroWindow InferOwnerOf(Window window)
        {
            var application = Application.Current;
            if (application == null)
            {
                return null;
            }

            var active = application.Windows.OfType<MetroWindow>().FirstOrDefault(x => x.IsActive);
            active = active ?? (PresentationSource.FromVisual((MetroWindow)application.MainWindow) == null ? null : (MetroWindow)application.MainWindow);
            return active == window ? null : active;
        }

        private bool ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings != null)
            {
                var type = target.GetType();

                foreach (var pair in settings)
                {
                    var propertyInfo = type.GetProperty(pair.Key);

                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(target, pair.Value, null);
                    }
                }

                return true;
            }

            return false;
        }


    }
}
