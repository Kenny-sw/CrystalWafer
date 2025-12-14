using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrystalTable.Forms
{
    /// <summary>
    /// Форма справки по горячим клавишам
    /// </summary>
    public class HotKeysForm : Form
    {
        public HotKeysForm()
        {
            InitializeForm();
            CreateContent();
        }

        private void InitializeForm()
        {
            Text = "Горячие клавиши";
            Size = new Size(650, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    Close();
            };
        }

        private void CreateContent()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                AutoScroll = true
            };

            var richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(250, 250, 250),
                Font = new Font("Segoe UI", 9.5f)
            };

            richTextBox.Rtf = GetHotKeysRtf();

            panel.Controls.Add(richTextBox);

            var btnClose = new Button
            {
                Text = "Закрыть",
                Dock = DockStyle.Bottom,
                Height = 35,
                DialogResult = DialogResult.OK
            };

            Controls.Add(panel);
            Controls.Add(btnClose);
            AcceptButton = btnClose;
        }

        private string GetHotKeysRtf()
        {
            // RTF заголовок с определением шрифтов и цветов
            string rtf = @"{\rtf1\ansi\deff0
{\fonttbl{\f0 Segoe UI;}{\f1 Consolas;}}
{\colortbl;\red51\green51\blue51;\red0\green102\blue204;\red46\green204\blue113;\red231\green76\blue60;\red241\green196\blue15;\red100\green100\blue100;}
\viewkind4\uc1\pard\sa100

\b\f0\fs28\cf2 ⌨ Справочник горячих клавиш\b0\fs20\cf1\par
\par

\b\fs22\cf2 📁 Файл\b0\fs20\cf1\par
\trowd\cellx2500\cellx6500
\intbl\b\f1 Ctrl+N\b0\f0\cell Создать новую карту пластины\cell\row
\intbl\b\f1 Ctrl+O\b0\f0\cell Открыть файл проекта\cell\row
\intbl\b\f1 Ctrl+S\b0\f0\cell Сохранить проект\cell\row
\intbl\b\f1 Ctrl+Shift+S\b0\f0\cell Сохранить как...\cell\row
\par

\b\fs22\cf2 ✏️ Редактирование\b0\fs20\cf1\par
\trowd\cellx2500\cellx6500
\intbl\b\f1 Ctrl+Z\b0\f0\cell Отменить последнее действие\cell\row
\intbl\b\f1 Ctrl+Y\b0\f0\cell Повторить отменённое действие\cell\row
\intbl\b\f1 Ctrl+A\b0\f0\cell Выделить все кристаллы\cell\row
\intbl\b\f1 Escape\b0\f0\cell Снять выделение\cell\row
\intbl\b\f1 Ctrl + клик\b0\f0\cell Добавить/убрать кристалл из выделения\cell\row
\par

\b\fs22\cf2 🔍 Масштаб и навигация\b0\fs20\cf1\par
\trowd\cellx2500\cellx6500
\intbl\b\f1 Ctrl++\b0\f0\cell Увеличить масштаб\cell\row
\intbl\b\f1 Ctrl+-\b0\f0\cell Уменьшить масштаб\cell\row
\intbl\b\f1 Ctrl+0\b0\f0\cell Сбросить масштаб (100%)\cell\row
\intbl\b\f1 Колёсико мыши\b0\f0\cell Изменение масштаба\cell\row
\intbl\b\f1 СКМ + перетаскивание\b0\f0\cell Панорамирование карты\cell\row
\par

\b\fs22\cf3 ✅ Bin Map (категории годности)\b0\fs20\cf1\par
\cf6\i Работают при выделенных кристаллах\i0\cf1\par
\trowd\cellx2500\cellx6500
\intbl\b\f1 1\b0\f0\cell ✅ Годен (Good)\cell\row
\intbl\b\f1 2\b0\f0\cell ❌ Брак (Defective)\cell\row
\intbl\b\f1 3\b0\f0\cell ❓ Проверить (Needs Review)\cell\row
\intbl\b\f1 4\b0\f0\cell 🔧 Доработка (Rework)\cell\row
\intbl\b\f1 5\b0\f0\cell 📐 Край (Edge)\cell\row
\intbl\b\f1 Delete / Backspace\b0\f0\cell Сбросить категорию (Not Inspected)\cell\row
\intbl\b\f1 B\b0\f0\cell Показать/скрыть Bin Map\cell\row
\par

\b\fs22\cf5 🔄 Автообход\b0\fs20\cf1\par
\cf6\i Работают во время автообхода\i0\cf1\par
\trowd\cellx2500\cellx6500
\intbl\b\f1 Space\b0\f0\cell Пауза / Продолжить обход\cell\row
\intbl\b\f1 Escape\b0\f0\cell Остановить обход\cell\row
\intbl\b\f1 ← (влево)\b0\f0\cell Перейти к предыдущему кристаллу (в режиме паузы)\cell\row
\intbl\b\f1 → (вправо)\b0\f0\cell Перейти к следующему кристаллу (в режиме паузы)\cell\row
\par

\b\fs22\cf2 🖱️ Мышь\b0\fs20\cf1\par
\trowd\cellx2500\cellx6500
\intbl\b\f1 ЛКМ\b0\f0\cell Выбор кристалла / начало выделения области\cell\row
\intbl\b\f1 ЛКМ + перетаскивание\b0\f0\cell Рамка выделения (выбор нескольких кристаллов)\cell\row
\intbl\b\f1 СКМ + перетаскивание\b0\f0\cell Панорамирование (перемещение вида)\cell\row
\intbl\b\f1 Колёсико\b0\f0\cell Изменение масштаба\cell\row
\intbl\b\f1 Двойной клик на кристалл\b0\f0\cell Переход к кристаллу\cell\row
\par

\b\fs22\cf4 📌 Миникарта\b0\fs20\cf1\par
\trowd\cellx2500\cellx6500
\intbl\b\f1 Клик на миникарте\b0\f0\cell Перейти к точке на основной карте\cell\row
\par

\b\fs22\cf6 💡 Подсказки\b0\fs20\cf1\par
• \b Ctrl\b0  удерживайте для множественного выделения кристаллов\par
• \b Shift+Click\b0  на кнопке Connect переключает упрощённый протокол COM\par
• Наведите курсор на кристалл для отображения его информации в статус-баре\par

}";
            return rtf;
        }
    }
}
