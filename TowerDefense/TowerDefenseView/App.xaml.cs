using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using TowerDefenseModel;
using TowerDefensePersistence;
using TowerDefenseViewModel;

namespace TowerDefenseView
{
    /// <summary>
    /// A program környezetét biztosító osztály
    /// </summary>
    public partial class App : Application
    {
        #region Fields
        private GameModel gameModel;
        private EditorModel editorModel;

        private GameViewModel gameViewModel;
        private EditorViewModel editorViewModel;

        private MainWindow view;
        private EditorPage editorPage;
        private GamePage gamePage;
        private HelpWindow helpWindow;
        private ExtraSettingsWindow extraSettingsWindow;

        private IPersistence gamePersistence, editorPersistence;

        private (int, int) originalMapSize;
        #endregion
        #region Constructor
        /// <summary>
        /// Konstruktor
        /// </summary>
        public App()
        {
            view = new MainWindow();
            editorPage = new EditorPage(view);
            gamePage = new GamePage(view);

            gamePersistence = new FilePersistence();
            gameModel = new GameModel(gamePersistence, new GameData(10, 10));
            gameViewModel = new GameViewModel(gameModel);
            gameViewModel.Active = true;
            gamePage.DataContext = gameViewModel;

            editorPersistence = new FilePersistence();
            editorModel = new EditorModel(editorPersistence);
            editorViewModel = new EditorViewModel(editorModel);
            editorViewModel.Active = false;
            editorPage.DataContext = editorViewModel;

            view.ContentFrame.Content = gamePage;

            helpWindow = new HelpWindow();
            extraSettingsWindow = new ExtraSettingsWindow();

            gameViewModel.DisplayHelp += GameViewModel_DisplayHelp;
            gameViewModel.PropertyChanged += GameViewModel_PropertyChanged;
            gameViewModel.SaveGame += GameViewModel_SaveGame;
            gameViewModel.LoadGame += GameViewModel_LoadGame;
            gameViewModel.NewGame += GameViewModel_NewGame;
            gameViewModel.SwitchToEditor += GameViewModel_SwitchToEditor;
            gameViewModel.InvalidAction += GameViewModel_InvalidAction;
            gameViewModel.PlayerTransaction += GameViewModel_PlayerTransaction;
            gameViewModel.ExitProgram += GameViewModel_ExitProgram;
            gameViewModel.Error += GameViewModel_Error;
            gameViewModel.CellInfoDisplay += GameViewModel_CellInfoDisplay;
            gameViewModel.GameOver += GameViewModel_GameOver;


            editorViewModel.InvalidAction += EditorViewModel_InvalidAction;
            editorViewModel.LoadMap += EditorViewModel_LoadMap;
            editorViewModel.SaveMap += EditorViewModel_SaveMap;
            editorViewModel.Error += EditorViewModel_Error;
            editorViewModel.ExitProgram += EditorViewModel_ExitProgram;
            editorViewModel.ExtraSettings += EditorViewModel_ExtraSettings;
            editorViewModel.SwitchToGame += EditorViewModel_SwitchToGame;
            editorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;
            editorViewModel.DisplayHelp += EditorViewModel_DisplayHelp;

            helpWindow.Closing += HelpWindow_Closing;

            extraSettingsWindow.okButton.Click += ExtraSettings_OKPressed;
            extraSettingsWindow.cancelButton.Click += ExtraSettings_CancelPressed;
            extraSettingsWindow.Closing += ExtraSettingsWindow_Closing;

            view.Closing += View_Closing;
            view.Closed += View_Closed;

            view.Show();
        }
        #endregion
        #region GameViewModel handler methods
        private void GameViewModel_CellInfoDisplay(object sender, UserInteractionEventArgs e)
        {
            MessageBox.Show(e.Message, "Cella adatok");
        }
        private void GameViewModel_Error(object sender, UserInteractionEventArgs e)
        {
            MessageBox.Show(e.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void GameViewModel_ExitProgram(object sender, EventArgs e)
        {
            view.Close();
        }

        private void GameViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(gameViewModel.GridCols) ||
                e.PropertyName == nameof(gameViewModel.GridRows))
            {
                gamePage.RefreshSizes();
            }
        }

        private async void GameViewModel_SaveGame(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Játék mentése";
                saveFileDialog.Filter = "save files (*.sav)|*.sav";
                if (saveFileDialog.ShowDialog() == true)
                {
                    await gameViewModel.Save(saveFileDialog.FileName);
                }
            }
            catch (FilePersistenceException)
            {
                MessageBox.Show("Nem sikerült elmenteni a fájlt!", "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GameViewModel_LoadGame(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Játék betöltése";
                openFileDialog.Filter = "save files (*.sav)|*.sav";
                if (openFileDialog.ShowDialog() == true)
                {
                    await gameViewModel.Load(openFileDialog.FileName);
                }

            }
            catch (FilePersistenceException)
            {
                MessageBox.Show("Nem sikerült betölteni a fájlt! A fájlt nem sikerült megnyitni, vagy hibás formátumú.", "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);                
            }
            catch (ModelLoadException) 
            {
                MessageBox.Show("A megadott fájlban illegális pályaállapot van.", "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void GameViewModel_NewGame(object sender, CancelEventArgs e)
        {
            if (MessageBoxResult.No ==
                MessageBox.Show("Biztos új játékot kezdesz? A régi játék adatai elvesznek.",
                                "Tower Defense", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                e.Cancel = true;
            }
        }

        private void GameViewModel_DisplayHelp(object sender, EventArgs e)
        {
            if (!helpWindow.IsActive)
            {
                helpWindow.Show();
            }

        }

        private void GameViewModel_SwitchToEditor(object sender, EventArgs e)
        {
            if (MessageBoxResult.Yes ==
                MessageBox.Show("Biztos átlépsz a szerkesztőbe? A játék adatai a programból való kilépésig megmaradnak.",
                                "Tower Defense", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                editorViewModel.Active = true;
                gameViewModel.Active = false;
                view.ContentFrame.Content = editorPage;
                editorPage.RefreshSizes();
            }
        }

        private void GameViewModel_InvalidAction(object sender, UserInteractionEventArgs e)
        {

            MessageBox.Show("Érvénytelen művelet: " + e.Message, "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void GameViewModel_PlayerTransaction(object sender, UserInteractionEventArgs e)
        {
            if (MessageBoxResult.No ==
                MessageBox.Show(e.Message, "Tranzakció", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                e.Cancel = true;
            }
        }

        private void GameViewModel_GameOver(object sender, UserInteractionEventArgs e)
        {
            MessageBox.Show(e.Message, "Játék vége");
        }


        #endregion
        #region EditorViewModel handler methods
        private void EditorViewModel_DisplayHelp(object sender, EventArgs e)
        {
            if (!helpWindow.IsActive)
            {
                helpWindow.Show();
            }
        }

        private void EditorViewModel_ExitProgram(object sender, EventArgs e)
        {
            view.Close();
        }

        private void EditorViewModel_Error(object sender, UserInteractionEventArgs e)
        {
            MessageBox.Show(e.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void EditorViewModel_SaveMap(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Pálya mentése";
                saveFileDialog.Filter = "save files (*.sav)|*.sav";
                if (saveFileDialog.ShowDialog() == true)
                {
                    await editorViewModel.Save(saveFileDialog.FileName);
                }
            }
            catch (FilePersistenceException)
            {
                MessageBox.Show("Nem sikerült elmenteni a fájlt!", "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditorViewModel_LoadMap(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Pálya betöltése";
                openFileDialog.Filter = "save files (*.sav)|*.sav";
                if (openFileDialog.ShowDialog() == true)
                {
                    await editorViewModel.Load(openFileDialog.FileName);
                }

            }
            catch (FilePersistenceException)
            {
                MessageBox.Show("Nem sikerült betölteni a fájlt! A fájlt nem sikerült megnyitni, vagy hibás formátumú.", "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);             
            }
            catch (ModelLoadException)
            {            
                MessageBox.Show("A megadott fájlban illegális pályaállapot van.\nFigyelem: a szerkesztőbe nem lehet megkezdett játékot betölteni.", "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);             
            }

        }


        private void EditorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(editorViewModel.GridCols) ||
                e.PropertyName == nameof(editorViewModel.GridRows))
            {
                editorPage.RefreshSizes();
            }
        }


        private void EditorViewModel_InvalidAction(object sender, UserInteractionEventArgs e)
        {
            MessageBox.Show("Érvénytelen művelet: " + e.Message, "Tower Defense", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private void EditorViewModel_SwitchToGame(object sender, EventArgs e)
        {
            if (MessageBoxResult.Yes ==
                MessageBox.Show("Biztos átlépsz a játékba? A pálya adatai a programból való kilépésig megmaradnak.",
                                "Tower Defense", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                editorViewModel.Active = false;
                gameViewModel.Active = true;
                view.ContentFrame.Content = gamePage;
                gamePage.RefreshSizes();
            }
        }

        private void EditorViewModel_InvalidMove(object sender, EventArgs e)
        {
            MessageBox.Show("Érvénytelen lépés!");

        }

        private void EditorViewModel_ExtraSettings(object sender, EventArgs e)
        {

            originalMapSize = (editorViewModel.GridRows, editorViewModel.GridCols);
            extraSettingsWindow.Rows = editorViewModel.GridRows;
            extraSettingsWindow.Cols = editorViewModel.GridCols;
            extraSettingsWindow.Money = editorViewModel.StartMoney;
            view.IsEnabled = false;
            extraSettingsWindow.Show();
        }

        #endregion
        #region App handler methods
        private void View_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void View_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBoxResult.No ==
                MessageBox.Show("Biztos ki akarsz lépni?", "Tower Defense", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                e.Cancel = true;
            }
        }
        #endregion
        #region Other windows' handler methods
        private void HelpWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            helpWindow.Hide();

        }

        private void ExtraSettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            extraSettingsWindow.Hide();
            e.Cancel = true;
        }

        private void ExtraSettings_OKPressed(object sender, EventArgs e)
        {
            extraSettingsWindow.Hide();
            if ((extraSettingsWindow.Rows, extraSettingsWindow.Cols) != originalMapSize)
            {
                editorViewModel.SetMapSize(extraSettingsWindow.Rows, extraSettingsWindow.Cols);
                editorPage.RefreshSizes();
            }

            editorViewModel.StartMoney = extraSettingsWindow.Money;
            view.IsEnabled = true;

        }

        private void ExtraSettings_CancelPressed(object sender, EventArgs e)
        {
            extraSettingsWindow.Hide();
            view.IsEnabled = true;
        }
        #endregion
    }
}
