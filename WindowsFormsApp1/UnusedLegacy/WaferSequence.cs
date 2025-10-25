using System;
using System.Collections.Generic;

namespace CrystalTable.Logic
{
    public class WaferSequence
    {
        private int currentNumber;
        public string PartyName { get; }
        private readonly List<string> usedNumbers = new List<string>();

        public WaferSequence(string partyName, int startNumber = 1)
        {
            PartyName = partyName;
            currentNumber = startNumber;
        }

        public string GenerateNextWaferNumber()
        {
            string number;
            do
            {
                number = $"{currentNumber:D3}";
                currentNumber++;
            } while (usedNumbers.Contains(number));

            usedNumbers.Add(number);
            return $"{PartyName}-{number}";
        }

        public void MarkNumberAsUsed(string number)
        {
            if (!usedNumbers.Contains(number))
            {
                usedNumbers.Add(number);
                
                // Обновляем currentNumber если добавленный номер больше текущего
                if (int.TryParse(number, out int num) && num >= currentNumber)
                {
                    currentNumber = num + 1;
                }
            }
        }

        public bool IsNumberUsed(string number)
        {
            return usedNumbers.Contains(number);
        }

        public void Reset()
        {
            currentNumber = 1;
            usedNumbers.Clear();
        }
    }
}