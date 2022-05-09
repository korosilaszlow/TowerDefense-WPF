using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TowerDefenseViewModel
{
    /// <summary>
    /// Az <see cref="INotifyPropertyChanged"/> interfacee-t megvalósító osztály. Öröklésre szánva.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>Default konstruktor</summary>
        protected ViewModelBase() { }
        /// <summary>Az <see cref="INotifyPropertyChanged"/> megvalósítása, jelzi, ha megváltozott egy Property</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Kiváltja a <see cref="PropertyChanged"/> eseményt adott nevű Property-re
        /// </summary>
        /// <param name="propertyName">A Property neve string-ként. Ha nem adunk meg paramétert, akkor alapértelmezetten a hívó tagnak a neveadódik át paraméterként.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
