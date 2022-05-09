using System.ComponentModel;

namespace TowerDefenseViewModel
{
    /// <summary>
    /// Üzenetet továbbító eseményargumentum
    /// </summary>
    public class UserInteractionEventArgs : CancelEventArgs
    {
        /// <summary> A továbbított üzenet </summary>
        public string Message { get; set; }
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="msg">A továbbított üzenet</param>
        public UserInteractionEventArgs(string msg)
        {
            Message = msg;
        }
    }

}
