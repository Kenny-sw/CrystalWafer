using System.Windows.Forms;
using System.Drawing;

using System.Windows.Forms;
using System.Drawing;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        private void InitializeUI()
        {
            // Создаем кнопки управления рабочим процессом
            createMapButton = new Button
            {
                Name = "createMapButton",
                Text = "Создать карту",
                Size = new Size(270, 30),
                Location = new Point(10, 25),
                UseVisualStyleBackColor = true
            };
            createMapButton.Click += createMapButton_Click;

            editMapButton = new Button
            {
                Name = "editMapButton",
                Text = "Редактировать карту",
                Size = new Size(270, 30),
                Location = new Point(10, 60),
                UseVisualStyleBackColor = true
            };
            editMapButton.Click += editMapButton_Click;

            startWaferButton = new Button
            {
                Name = "startWaferButton",
                Text = "Начать пластину",
                Size = new Size(270, 30),
                Location = new Point(10, 95),
                UseVisualStyleBackColor = true
            };
            startWaferButton.Click += startWaferButton_Click;

            completeWaferButton = new Button
            {
                Name = "completeWaferButton",
                Text = "Завершить пластину",
                Size = new Size(270, 30),
                Location = new Point(10, 130),
                UseVisualStyleBackColor = true
            };
            completeWaferButton.Click += completeWaferButton_Click;

            archivePartyButton = new Button
            {
                Name = "archivePartyButton",
                Text = "Архивировать партию",
                Size = new Size(270, 30),
                Location = new Point(10, 165),
                UseVisualStyleBackColor = true
            };
            archivePartyButton.Click += archivePartyButton_Click;

            // Добавляем кнопки в groupBox
            groupBoxMainControl.Controls.AddRange(new Control[] {
                createMapButton,
                editMapButton,
                startWaferButton,
                completeWaferButton,
                archivePartyButton
            });

            // Устанавливаем начальное состояние видимости
            UpdateUI();
        }
    }
}