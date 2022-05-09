using System;
using System.Windows.Input;

namespace TowerDefenseViewModel
{
    /// <summary>
    /// Az ICommand interface-t megvalósító osztály
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Konstruktor csak végrehajtandó művelettel
        /// </summary>
        /// <param name="execute">A végrehajtandó művelet (adott objektum paraméterrel)</param>
        public DelegateCommand(Action<object> execute) : this(null, execute) { }

        /// <summary>
        /// Konstruktor végrehajtandó művelettel és végrehajthatósági feltétellel
        /// </summary>
        /// <param name="canExecute">Predikátumfüggvény, ami megadja, hogy végrehajtható-e a művelet adott objektumon</param>
        /// <param name="execute">A végrehajtandó művelet (adott objektum paraméterrel)</param>
        /// <exception cref="ArgumentNullException">Ha <see cref="execute"/> értéke <c>null</c></exception>
        public DelegateCommand(Func<object, bool> canExecute, Action<object> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }
        /// <summary>
        /// Esemény, ami jelzi ha változott <see cref="CanExecute(object)"/> értéke
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Adott objektum esetén megadja, hogy végrehajtható-e a művelet az <see cref="Execute(object)"/> metódussal
        /// </summary>
        /// <param name="parameter">Az objektum, amivel meghívnánk az <see cref="Execute(object)"/> metódust</param>
        /// <returns>Igaz, ha végrehajtható a művelet, hamis, ha nem</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        /// <summary>
        /// Végrehajtja a konstruáláskor megadott műveletet
        /// </summary>
        /// <param name="parameter">A művelethez paraméterként megadott objektum</param>
        /// <exception cref="InvalidOperationException">Ha <see cref="CanExecute(object)"/>=<c>false</c></exception>
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Command execution is disabled.");
            }
            _execute(parameter);
        }
        /// <summary>
        /// Manuálisan kiváltja a <see cref="CanExecuteChanged"/> eseményt
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
