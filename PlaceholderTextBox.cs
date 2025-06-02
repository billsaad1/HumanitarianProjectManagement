using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HumanitarianProjectManagement.UI.Controls // Assuming a namespace for UI controls
{
    public class PlaceholderTextBox : TextBox
    {
        private string _placeholderText;
        private bool _isPlaceholderActive;

        [Description("The placeholder text to be displayed when the control has no text.")]
        [Category("Appearance")]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                // If not yet created, handle will be zero, and we shouldn't try to update.
                // The placeholder will be applied in OnHandleCreated or when text is next checked.
                if (this.IsHandleCreated && string.IsNullOrEmpty(this.Text))
                {
                    SetPlaceholder();
                }
            }
        }

        public PlaceholderTextBox()
        {
            _placeholderText = "";
            // Attach event handlers
            this.GotFocus += (sender, e) => RemovePlaceholder();
            this.LostFocus += (sender, e) => SetPlaceholder();
            this.TextChanged += OnActualTextChanged;

            // Initial check in case text is set before placeholder
             SetPlaceholder(); // Call initially to set if empty
        }
        
        private void OnActualTextChanged(object sender, EventArgs e)
        {
            // If the text is changed by the user (not by placeholder logic)
            // and placeholder is active, we should remove placeholder state.
            // However, TextChanged also fires when we set/remove placeholder.
            // This check needs to be careful not to interfere with GotFocus/LostFocus.
            if (!string.IsNullOrEmpty(this.Text) && _isPlaceholderActive && this.Text != _placeholderText)
            {
                // User has typed something, so placeholder should not be active
                _isPlaceholderActive = false;
                // No need to change ForeColor here as it's handled by GotFocus/LostFocus
            }
        }


        private void SetPlaceholder()
        {
            if (string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(PlaceholderText) && !this.Focused)
            {
                this.Text = PlaceholderText;
                this.ForeColor = Color.Gray;
                _isPlaceholderActive = true;
            }
        }

        private void RemovePlaceholder()
        {
            if (_isPlaceholderActive && !string.IsNullOrEmpty(PlaceholderText))
            {
                if (this.Text == PlaceholderText) // Only clear if it's the placeholder
                {
                    this.Text = "";
                }
                this.ForeColor = SystemColors.WindowText; // Use default text color
                _isPlaceholderActive = false;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!string.IsNullOrEmpty(PlaceholderText) && string.IsNullOrEmpty(this.Text))
            {
                SetPlaceholder();
            }
        }
        
        // Override Text property to ensure placeholder logic is considered
        public override string Text
        {
            get
            {
                if (_isPlaceholderActive && base.Text == _placeholderText)
                    return ""; // Return empty if placeholder is active and text is placeholder
                return base.Text;
            }
            set
            {
                // If value is empty and we have placeholder text, we might need to show placeholder
                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_placeholderText) && !this.Focused)
                {
                    base.Text = value; // set it to empty first
                    SetPlaceholder(); // then apply placeholder logic
                }
                else
                {
                    // If text is being set, and it's not the placeholder, remove placeholder state
                    if (_isPlaceholderActive && value != _placeholderText)
                    {
                         _isPlaceholderActive = false;
                         this.ForeColor = SystemColors.WindowText; 
                    }
                    base.Text = value;
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // The WM_PAINT message is sent when the window needs to be repainted.
            // This is a common place to add custom drawing.
            if (m.Msg == 0x000F && _isPlaceholderActive && string.IsNullOrEmpty(base.Text) && !this.Focused && !string.IsNullOrEmpty(PlaceholderText))
            {
                 // This is a fallback; SetPlaceholder should handle text and color.
                 // If, for some reason, base.Text is empty but placeholder isn't showing,
                 // this could re-trigger it.
                 // However, careful not to cause infinite loops.
                 // SetPlaceholder(); // Potentially risky here without more checks
            }
        }
    }
}
