from pathlib import Path
import difflib
old = Path(r"WindowsFormsApp1/Arduino/StepByStepOOP1.3.ino").read_text(encoding='utf-8', errors='ignore').splitlines()
new = Path(r"WindowsFormsApp1/Arduino/StepByStepOOP1.4.ino").read_text(encoding='utf-8', errors='ignore').splitlines()
for line in difflib.unified_diff(old, new, fromfile='StepByStepOOP1.3.ino', tofile='StepByStepOOP1.4.ino', n=3):
    print(line)
