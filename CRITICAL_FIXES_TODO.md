# 🔥 КРИТИЧЕСКИЕ ЗАДАЧИ (ACTION ITEMS)

## 1. ИСПРАВИТЬ НЕМЕДЛЕННО

### ❌ CS0649 Warning: cmbTemplateSelector не инициализирован

**Файл:** `Form1.MapBuilder.cs:17`

**Проблема:**
```csharp
private ComboBox cmbTemplateSelector;  // Объявлено, но НИКОГДА не создается
```

**Используется в:**
- `ShowInitialState()` - проверка `if (cmbTemplateSelector != null)`
- `LoadTemplateList()` - `cmbTemplateSelector.Items.Clear()`

**Решение 1 (если нужен):**
```csharp
// В CreateWorkflowPanel() добавить:
cmbTemplateSelector = new ComboBox
{
    Location = new Point(85, 5),
    Width = 200,
    DropDownStyle = ComboBoxStyle.DropDownList
};
cmbTemplateSelector.SelectedIndexChanged += CmbTemplateSelector_SelectedIndexChanged;
selectorPanel.Controls.Add(cmbTemplateSelector);
```

**Решение 2 (если не нужен):**
```csharp
// Удалить поле и все упоминания:
- private ComboBox cmbTemplateSelector;
- if (cmbTemplateSelector != null) cmbTemplateSelector.Visible = false;
- LoadTemplateList() { ... }
- CmbTemplateSelector_SelectedIndexChanged() { ... }
```

---

### ❌ Утечка ресурсов: WorkflowController не освобождается

**Файл:** `Form1.cs`

**Проблема:**
```csharp
private WorkflowController workflowController;

private void Form1_FormClosed(...)
{
    serialPortController?.Dispose();
    waferController?.Dispose();
    // workflowController НЕ освобождается!
}
```

**Решение:**
```csharp
private void Form1_FormClosed(object sender, FormClosedEventArgs e)
{
    // Отписаться от событий
    if (workflowController != null)
    {
        workflowController.StateChanged -= OnWorkflowStateChanged;
    }
    
    // Освободить ресурсы
    serialPortController?.Dispose();
    waferController?.Dispose();
    workflowController?.Dispose();
    
    // Освободить динамически созданные UI элементы
    workflowPanel?.Dispose();
    mapBuilderPanel?.Dispose();
}
```

---

### ❌ Двойная подписка на событие Create.Click

**Файл:** `Form1.MapBuilder.cs`

**Проблема:**
```csharp
private void IntegrateCreateButton()
{
    if (Create != null)
    {
        Create.Click -= Create_Click;  // Отписка только от ОДНОГО обработчика
        Create.Click += (s, e) => { ... };  // Новая подписка
        
        // Если метод вызывается дважды, будет ДВЕ лямбды!
    }
}
```

**Решение:**
```csharp
private EventHandler createButtonHandler;

private void IntegrateCreateButton()
{
    if (Create == null) return;
    
    // Отписаться от ВСЕХ обработчиков
    Create.Click -= Create_Click;
    if (createButtonHandler != null)
        Create.Click -= createButtonHandler;
    
    // Создать новый обработчик и сохранить ссылку
    createButtonHandler = (s, e) => 
    {
        if (!string.IsNullOrWhiteSpace(SizeX.Text) && 
            !string.IsNullOrWhiteSpace(SizeY.Text) &&
            !string.IsNullOrWhiteSpace(WaferDiameter.Text))
        {
            CreateMapFromCurrentInputs();
        }
        else
        {
            MessageBox.Show("Заполните параметры пластины перед созданием карты.",
                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    };
    
    Create.Click += createButtonHandler;
    Create.Text = "Создать карту";
}
```

---

## 2. ИСПРАВИТЬ В БЛИЖАЙШЕЕ ВРЕМЯ

### ⚠️ Пустые catch блоки проглатывают ошибки

**Найдено в 15+ местах:**

```csharp
// Form1.cs
try { InitializeCalibrationUiState(); } catch { }
try { UpdateBuildMapEnabled(); } catch { }
try { UpdateCalibrationLabelsAfterBuild(); } catch { }

// Form1.MapBuilder.cs
try { UpdateWorkflowUI(); } catch { }
```

**Решение:**
```csharp
try 
{ 
    InitializeCalibrationUiState(); 
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Calibration init failed: {ex}");
    // Или:
    AppLogger.Log($"Error in InitializeCalibrationUiState: {ex.Message}", LogLevel.Error);
}
```

---

### ⚠️ XML десериализация без валидации

**Файл:** `WaferTemplateManager.cs`

**Проблема:**
```csharp
private WaferTemplate LoadTemplate(string filePath)
{
    var serializer = new XmlSerializer(typeof(WaferTemplate));
    using (var reader = new StreamReader(filePath))
    {
        return (WaferTemplate)serializer.Deserialize(reader);
        // ← Уязвимости: XML Bomb, XXE
    }
}
```

**Решение:**
```csharp
private WaferTemplate LoadTemplate(string filePath)
{
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
        MaxCharactersFromEntities = 1024,
        XmlResolver = null,
        MaxCharactersInDocument = 10_000_000  // 10 MB
    };
    
    var serializer = new XmlSerializer(typeof(WaferTemplate));
    
    try
    {
        using (var xmlReader = XmlReader.Create(filePath, settings))
        {
            return (WaferTemplate)serializer.Deserialize(xmlReader);
        }
    }
    catch (Exception ex)
    {
        AppLogger.Log($"Failed to load template from {filePath}: {ex.Message}", LogLevel.Error);
        return null;
    }
}
```

---

### ⚠️ UpdateUI() вызывается слишком часто

**Проблема:**
```csharp
// Вызывается при:
- MouseMove (сотни раз в секунду)
- MouseWheel
- KeyDown
- Zoom
- Каждой команде Undo/Redo
- Изменении MapBuilder параметров
```

**Решение (Throttle):**
```csharp
public partial class Form1
{
    private System.Windows.Forms.Timer updateThrottleTimer;
    private bool updatePending;
    
    private void InitializeUpdateThrottle()
    {
        updateThrottleTimer = new Timer 
        { 
            Interval = 16,  // ~60 FPS
            Enabled = false 
        };
        updateThrottleTimer.Tick += (s, e) =>
        {
            updateThrottleTimer.Stop();
            updatePending = false;
            PerformActualUpdate();
        };
    }
    
    public void UpdateUI()
    {
        if (updatePending) return;
        
        updatePending = true;
        updateThrottleTimer.Stop();
        updateThrottleTimer.Start();
    }
    
    private void PerformActualUpdate()
    {
        pictureBox1.Invalidate();
        uiController.UpdateStatusBar(waferController, zoomPanController);
        uiController.UpdateSelectionLabel(mouseController.SelectedCrystals);
        uiController.UpdateToolbarState(commandHistory);
        
        try { UpdateWorkflowUI(); } 
        catch (Exception ex) 
        { 
            AppLogger.Log($"UpdateWorkflowUI failed: {ex.Message}", LogLevel.Warning); 
        }
    }
}
```

---

## 3. ОПТИМИЗАЦИИ

### 💡 Кэширование Bitmap

**Файл:** `WaferBitmapRenderer.cs` (предположительно)

**Проблема:**
```csharp
public Bitmap RenderWafer(...)
{
    var bitmap = new Bitmap(width, height);  // КАЖДЫЙ РАЗ новый!
    using (var g = Graphics.FromImage(bitmap))
    {
        // Долгое рисование...
    }
    return bitmap;
}
```

**Решение:**
```csharp
public class WaferBitmapRenderer
{
    private Bitmap cachedBitmap;
    private Rectangle lastBounds;
    private bool isDirty = true;
    
    public void Invalidate()
    {
        isDirty = true;
    }
    
    public Bitmap RenderWafer(Rectangle bounds, ...)
    {
        if (!isDirty && 
            cachedBitmap != null && 
            lastBounds == bounds)
        {
            return cachedBitmap;
        }
        
        cachedBitmap?.Dispose();
        cachedBitmap = new Bitmap(bounds.Width, bounds.Height);
        
        using (var g = Graphics.FromImage(cachedBitmap))
        {
            // Рисование...
        }
        
        lastBounds = bounds;
        isDirty = false;
        return cachedBitmap;
    }
    
    public void Dispose()
    {
        cachedBitmap?.Dispose();
    }
}
```

---

### 💡 Рефакторинг Singleton CrystalManager

**Файл:** `CrystalManager.cs`

**Проблема:**
```csharp
public class CrystalManager
{
    private static CrystalManager instance;
    public static CrystalManager Instance => instance ?? (instance = new CrystalManager());
    
    // НЕ thread-safe!
    // Нельзя освободить!
    // Глобальное состояние!
}
```

**Решение:**
```csharp
// 1. Сделать обычным классом (без Singleton)
public class CrystalManager : IDisposable
{
    private List<Crystal> crystals = new List<Crystal>();
    
    public IReadOnlyList<Crystal> Crystals => crystals.AsReadOnly();
    
    public void Clear()
    {
        crystals.Clear();
    }
    
    public void Dispose()
    {
        Clear();
    }
}

// 2. Передавать через конструкторы
public class WaferController
{
    private readonly CrystalManager crystalManager;
    
    public WaferController(CrystalManager crystalManager)
    {
        this.crystalManager = crystalManager ?? throw new ArgumentNullException();
    }
}

// 3. В Form1
public partial class Form1
{
    private readonly CrystalManager crystalManager;
    
    public Form1()
    {
        InitializeComponent();
        
        crystalManager = new CrystalManager();
        waferController = new WaferController(crystalManager);
        mouseController = new MouseController(this, waferController, crystalManager);
        // ...
    }
    
    private void Form1_FormClosed(...)
    {
        crystalManager?.Dispose();
        waferController?.Dispose();
        // ...
    }
}
```

---

## 📋 CHECKLIST НЕМЕДЛЕННЫХ ДЕЙСТВИЙ

```
Критично (сегодня):
☐ Исправить warning CS0649 (cmbTemplateSelector)
☐ Добавить Dispose для workflowController
☐ Исправить двойную подписку Create.Click

Важно (эта неделя):
☐ Заменить пустые catch на catch с логированием
☐ Добавить валидацию XML десериализации
☐ Добавить throttle для UpdateUI()

Оптимизация (в течение месяца):
☐ Добавить кэширование Bitmap
☐ Рефакторинг CrystalManager (убрать Singleton)
☐ Добавить интерфейсы для контроллеров
☐ Написать интеграционные тесты
```

---

## 🎯 ПРИОРИТИЗАЦИЯ

### HIGH PRIORITY (критично)
1. ✅ **Утечки ресурсов** - может привести к краху приложения
2. ✅ **Двойная подписка** - некорректное поведение UI
3. ✅ **Warning компилятора** - потенциальный NullReferenceException

### MEDIUM PRIORITY (важно)
1. ⚠️ **Пустые catch** - невозможность диагностики ошибок
2. ⚠️ **XML валидация** - безопасность
3. ⚠️ **UpdateUI throttle** - производительность

### LOW PRIORITY (желательно)
1. 💡 **Bitmap кэширование** - оптимизация
2. 💡 **Singleton рефакторинг** - архитектура
3. 💡 **Интерфейсы** - тестируемость

---

## 📞 КОНТАКТЫ И ПОМОЩЬ

Если нужна помощь с реализацией:
- Создайте отдельную ветку `fix/critical-issues`
- Коммитьте изменения постепенно
- Тестируйте каждое исправление отдельно
- Создавайте Pull Request для ревью

**Успехов в рефакторинге!** 🚀
