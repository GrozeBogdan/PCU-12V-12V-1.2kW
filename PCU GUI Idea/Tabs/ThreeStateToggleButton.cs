using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace PCU_GUI_Idea.Tabs
{
    public enum ToggleButtonState
    {
        Unchecked,
        Checked,
        Indeterminate
    }

    public class ThreeStateToggleButton : ToggleButton
    {
        // Define a dependency property to store the state
        public static readonly DependencyProperty ToggleStateProperty =
            DependencyProperty.Register("ToggleState", typeof(ToggleButtonState), typeof(ThreeStateToggleButton), new PropertyMetadata(ToggleButtonState.Unchecked, OnToggleStateChanged));

        public ToggleButtonState ToggleState
        {
            get { return (ToggleButtonState)GetValue(ToggleStateProperty); }
            set { SetValue(ToggleStateProperty, value); }
        }

        private static void OnToggleStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ThreeStateToggleButton;
            button?.UpdateVisualState();
        }

        // Update the visual state based on the current state
        private void UpdateVisualState()
        {
            // Logic to update button content or state-related styles
            switch (ToggleState)
            {
                case ToggleButtonState.Checked:
                    this.Content = "Checked"; // Can change to any desired text or style
                    break;
                case ToggleButtonState.Unchecked:
                    this.Content = "Unchecked"; // Change text
                    break;
                case ToggleButtonState.Indeterminate:
                    this.Content = "Indeterminate"; // Change text
                    break;
            }
        }

        protected override void OnClick()
        {
            base.OnClick();

            // Cycle through the states on click
            if (ToggleState == ToggleButtonState.Indeterminate)
            {
                ToggleState = ToggleButtonState.Checked;
            }
            else
            {
                ToggleState++;
            }
        }
    }
}
