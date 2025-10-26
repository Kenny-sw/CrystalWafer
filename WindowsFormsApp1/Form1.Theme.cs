using System.Drawing;
using System.Windows.Forms;

namespace CrystalTable
{
    /// <summary>
    /// Управление темами оформления интерфейса
    /// </summary>
    public partial class Form1
    {
        private bool isDarkTheme = false;

        // Цвета светлой темы
        private static class LightTheme
        {
            public static readonly Color FormBackground = Color.FromArgb(240, 240, 240);
            public static readonly Color PictureBoxBackground = Color.White;
            public static readonly Color PanelBackground = Color.FromArgb(246, 250, 246);
            public static readonly Color GroupBoxBackground = SystemColors.Control;
            public static readonly Color TextColor = Color.Black;
            public static readonly Color MenuBackground = SystemColors.Menu;
            public static readonly Color ToolStripBackground = SystemColors.Control;
            public static readonly Color StatusStripBackground = SystemColors.Control;
        }

        // Цвета темной темы
        private static class DarkTheme
        {
            public static readonly Color FormBackground = Color.FromArgb(30, 30, 30);
            public static readonly Color PictureBoxBackground = Color.FromArgb(45, 45, 45);
            public static readonly Color PanelBackground = Color.FromArgb(40, 40, 40);
            public static readonly Color GroupBoxBackground = Color.FromArgb(50, 50, 50);
            public static readonly Color TextColor = Color.FromArgb(220, 220, 220);
            public static readonly Color MenuBackground = Color.FromArgb(35, 35, 35);
            public static readonly Color ToolStripBackground = Color.FromArgb(35, 35, 35);
            public static readonly Color StatusStripBackground = Color.FromArgb(35, 35, 35);
        }

        /// <summary>
        /// Переключение между светлой и темной темой
        /// </summary>
        private void ToggleTheme()
        {
            isDarkTheme = !isDarkTheme;
            ApplyTheme(isDarkTheme);
        }

        /// <summary>
        /// Применение темы ко всем элементам интерфейса
        /// </summary>
        private void ApplyTheme(bool dark)
        {
            if (dark)
            {
                ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
            }

            // Перерисовать карту с новой темой
            pictureBox1.Invalidate();
        }

        /// <summary>
        /// Применение светлой темы
        /// </summary>
        private void ApplyLightTheme()
        {
            // Основная форма
            this.BackColor = LightTheme.FormBackground;
            this.ForeColor = LightTheme.TextColor;

            // PictureBox (карта)
            pictureBox1.BackColor = LightTheme.PictureBoxBackground;

            // Панели
            mainPanel.BackColor = LightTheme.FormBackground;
            rightPanel.BackColor = LightTheme.FormBackground;

            if (mapBuilderPanel != null)
            {
                mapBuilderPanel.BackColor = LightTheme.PanelBackground;
            }

            // Группы
            ApplyThemeToGroupBox(groupBoxMainControl, false);
            ApplyThemeToGroupBox(groupBoxConnection, false);
            ApplyThemeToGroupBox(groupBoxManualControl, false);

            // Меню и панели инструментов
            menuStrip1.BackColor = LightTheme.MenuBackground;
            menuStrip1.ForeColor = LightTheme.TextColor;
            toolStrip1.BackColor = LightTheme.ToolStripBackground;
            toolStrip1.ForeColor = LightTheme.TextColor;
            statusStrip1.BackColor = LightTheme.StatusStripBackground;
            statusStrip1.ForeColor = LightTheme.TextColor;

            // Применить ко всем дочерним элементам
            ApplyThemeToControls(this.Controls, false);
        }

        /// <summary>
        /// Применение темной темы
        /// </summary>
        private void ApplyDarkTheme()
        {
            // Основная форма
            this.BackColor = DarkTheme.FormBackground;
            this.ForeColor = DarkTheme.TextColor;

            // PictureBox (карта)
            pictureBox1.BackColor = DarkTheme.PictureBoxBackground;

            // Панели
            mainPanel.BackColor = DarkTheme.FormBackground;
            rightPanel.BackColor = DarkTheme.FormBackground;

            if (mapBuilderPanel != null)
            {
                mapBuilderPanel.BackColor = DarkTheme.PanelBackground;
            }

            // Группы
            ApplyThemeToGroupBox(groupBoxMainControl, true);
            ApplyThemeToGroupBox(groupBoxConnection, true);
            ApplyThemeToGroupBox(groupBoxManualControl, true);

            // Меню и панели инструментов
            menuStrip1.BackColor = DarkTheme.MenuBackground;
            menuStrip1.ForeColor = DarkTheme.TextColor;
            menuStrip1.Renderer = new ToolStripProfessionalRenderer(new DarkThemeColorTable());
            
            toolStrip1.BackColor = DarkTheme.ToolStripBackground;
            toolStrip1.ForeColor = DarkTheme.TextColor;
            toolStrip1.Renderer = new ToolStripProfessionalRenderer(new DarkThemeColorTable());
            
            statusStrip1.BackColor = DarkTheme.StatusStripBackground;
            statusStrip1.ForeColor = DarkTheme.TextColor;
            statusStrip1.Renderer = new ToolStripProfessionalRenderer(new DarkThemeColorTable());

            // Применить ко всем дочерним элементам
            ApplyThemeToControls(this.Controls, true);
        }

        /// <summary>
        /// Применение темы к GroupBox
        /// </summary>
        private void ApplyThemeToGroupBox(GroupBox groupBox, bool dark)
        {
            if (groupBox == null) return;

            groupBox.BackColor = dark ? DarkTheme.GroupBoxBackground : LightTheme.GroupBoxBackground;
            groupBox.ForeColor = dark ? DarkTheme.TextColor : LightTheme.TextColor;

            // Применить к дочерний элементам
            ApplyThemeToControls(groupBox.Controls, dark);
        }

        /// <summary>
        /// Рекурсивное применение темы ко всем контролам
        /// </summary>
        private void ApplyThemeToControls(Control.ControlCollection controls, bool dark)
        {
            foreach (Control control in controls)
            {
                // Пропускаем специальные элементы
                if (control is MenuStrip || control is ToolStrip || control is StatusStrip)
                {
                    continue;
                }

                // Пропускаем кнопку фиксации (у неё свои цвета)
                if (control.Name == "buttonLockToggle")
                {
                    control.ForeColor = dark ? DarkTheme.TextColor : LightTheme.TextColor;
                    continue;
                }

                // Применяем тему
                if (control is Button || control is Label || control is CheckBox || 
                    control is RadioButton || control is GroupBox)
                {
                    control.BackColor = dark ? DarkTheme.GroupBoxBackground : LightTheme.GroupBoxBackground;
                    control.ForeColor = dark ? DarkTheme.TextColor : LightTheme.TextColor;
                }
                else if (control is TextBox || control is ComboBox || control is NumericUpDown)
                {
                    control.BackColor = dark ? Color.FromArgb(60, 60, 60) : Color.White;
                    control.ForeColor = dark ? DarkTheme.TextColor : LightTheme.TextColor;
                }
                else if (control is Panel)
                {
                    if (control != mainPanel && control != rightPanel)
                    {
                        control.BackColor = dark ? DarkTheme.PanelBackground : LightTheme.PanelBackground;
                        control.ForeColor = dark ? DarkTheme.TextColor : LightTheme.TextColor;
                    }
                }

                // Рекурсия для вложенных контролов
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls, dark);
                }
            }
        }

        /// <summary>
        /// Обработчик переключения темы из меню
        /// </summary>
        private void darkThemeToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            // Переключаем тему (состояние checkbox уже обновлено автоматически через CheckOnClick)
            isDarkTheme = darkThemeToolStripMenuItem.Checked;
            ApplyTheme(isDarkTheme);
        }

        /// <summary>
        /// Кастомная цветовая схема для темной темы
        /// </summary>
        private class DarkThemeColorTable : ProfessionalColorTable
        {
            public override Color MenuBorder => Color.FromArgb(60, 60, 60);
            public override Color MenuItemBorder => Color.FromArgb(80, 80, 80);
            public override Color MenuItemSelected => Color.FromArgb(60, 60, 60);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 60);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, 60, 60);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(50, 50, 50);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(50, 50, 50);
            public override Color ToolStripDropDownBackground => Color.FromArgb(40, 40, 40);
            public override Color ImageMarginGradientBegin => Color.FromArgb(40, 40, 40);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(40, 40, 40);
            public override Color ImageMarginGradientEnd => Color.FromArgb(40, 40, 40);
        }
    }
}
