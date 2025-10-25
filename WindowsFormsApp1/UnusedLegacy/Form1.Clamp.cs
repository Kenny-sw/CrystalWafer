using System.Threading.Tasks;
using System.Windows.Forms;
using CrystalTable.Logic;

using CrystalTable.Models;

namespace CrystalTable
{
    public partial class Form1 : Form
    {
        private bool clampEngaged;

        private async void fixationButton_Click(object sender, System.EventArgs e)
        {
            await SetClampStateAsync(true);
        }

        private async void resetButton_Click(object sender, System.EventArgs e)
        {
            await SetClampStateAsync(false);
        }

        private async Task SetClampStateAsync(bool engage)
        {
            uint payload = engage ? 1u : 0u;

            if (!await TrySendAsync(Protocol.Commands.SetClamp, payload))
            {
                return;
            }

            clampEngaged = engage;
            UpdateClampUi();
        }

        private void UpdateClampUi()
        {
            if (fixationButton != null)
            {
                fixationButton.Enabled = !clampEngaged;
            }

            if (resetButton != null)
            {
                resetButton.Enabled = clampEngaged;
            }
        }

        private void InitializeClampUi()
        {
            clampEngaged = false;
            UpdateClampUi();
        }
    }
}

