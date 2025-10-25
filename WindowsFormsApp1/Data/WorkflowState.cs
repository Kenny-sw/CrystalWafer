namespace CrystalTable.Data
{
    /// <summary>
    /// Состояния рабочего процесса
    /// </summary>
    public enum WorkflowState
    {
        /// <summary>
        /// Начальное состояние - нет активной карты
        /// </summary>
        Initial,

        /// <summary>
        /// Редактирование черновика карты
        /// </summary>
        DraftEditing,

        /// <summary>
        /// Карта создана и готова к работе
        /// </summary>
        MapReady,

        /// <summary>
        /// Работа с пластиной
        /// </summary>
        WaferProcessing,

        /// <summary>
        /// Редактирование существующей карты
        /// </summary>
        MapEditing
    }
}
