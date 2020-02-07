using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DynHostUpdater.Controls
{
    /// <summary>
    ///     ConsoleOutput
    /// </summary>
    /// <seealso cref="TextBox" />
    public class ConsoleOutput : TextBox
    {
        #region Fields

        public static readonly DependencyProperty MaxConsoleLinesProperty = DependencyProperty.Register(
            nameof(MaxConsoleLines), typeof(int), typeof(ConsoleOutput), new PropertyMetadata(100));

        private readonly TextBoxOutputTextWriter _textBoxOutputTextWriter;

        #endregion

        #region Ctors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConsoleOutput" /> class.
        /// </summary>
        public ConsoleOutput()
        {
            _textBoxOutputTextWriter = new TextBoxOutputTextWriter(this);
            IsReadOnly = true;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            TextWrapping = TextWrapping.Wrap;
            TextAlignment = TextAlignment.Left;
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the maximum console lines.
        /// </summary>
        /// <value>
        /// The maximum console lines.
        /// </value>
        public int MaxConsoleLines
        {
            get => (int) GetValue(MaxConsoleLinesProperty);
            set => SetValue(MaxConsoleLinesProperty, value);
        }

        #endregion
    }


    /// <summary>
    ///     TextBoxOutputter
    /// </summary>
    /// <seealso cref="TextWriter" />
    public class TextBoxOutputTextWriter : TextWriter
    {
        #region Fields

        private const string _return = "\r\n";
        private readonly ConsoleOutput _console;

        #endregion

        #region Ctors

        public TextBoxOutputTextWriter(ConsoleOutput output)
        {
            _console = output;
            Console.SetOut(this);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     When overridden in a derived class, returns the character encoding in which the output is written.
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        #endregion

        #region Methods

        /// <summary>
        ///     Writes a character to the text stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        public override void Write(char value)
        {
            base.Write(value);
            
            _console.Dispatcher?.BeginInvoke(new Action(() =>
            {
                
                // bad find another solution
                var lines = _console.Text.Split(_return);
                if (lines.Length > _console.MaxConsoleLines)
                {
                    var lineReplace = lines[0] + _return;

                    var newTxt = _console.Text.Replace(lineReplace, string.Empty);

                    _console.Text = newTxt;
                }

                _console.AppendText(value.ToString());
                _console.LineDown();
            }));
        }

        #endregion
    }
}