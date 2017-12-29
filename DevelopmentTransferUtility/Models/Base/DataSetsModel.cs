using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace NpoComputer.DevelopmentTransferUtility.Models.Base
{
  /// <summary>
  /// Модель детальных разделов.
  /// </summary>
  public class DataSetsModel
  {
    /// <summary>
    /// Словарь наборов данных.
    /// </summary>
    private readonly IDictionary<int, DataSetModel> datasets = new Dictionary<int, DataSetModel>();

    /// <summary>
    /// 1 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet1")]
    public DataSetModel DetailDataSet1
    {
      get { return this.GetDataSetByIndex(1); }

      set { this.AddOrSetDataSet(1, value); }
    }

    /// <summary>
    /// 2 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet2")]
    public DataSetModel DetailDataSet2
    {
      get { return this.GetDataSetByIndex(2); }

      set { this.AddOrSetDataSet(2, value); }
    }

    /// <summary>
    /// 3 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet3")]
    public DataSetModel DetailDataSet3
    {
      get { return this.GetDataSetByIndex(3); }

      set { this.AddOrSetDataSet(3, value); }
    }

    /// <summary>
    /// 4 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet4")]
    public DataSetModel DetailDataSet4
    {
      get { return this.GetDataSetByIndex(4); }

      set { this.AddOrSetDataSet(4, value); }
    }

    /// <summary>
    /// 5 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet5")]
    public DataSetModel DetailDataSet5
    {
      get { return this.GetDataSetByIndex(5); }

      set { this.AddOrSetDataSet(5, value); }
    }

    /// <summary>
    /// 6 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet6")]
    public DataSetModel DetailDataSet6
    {
      get { return this.GetDataSetByIndex(6); }

      set { this.AddOrSetDataSet(6, value); }
    }

    /// <summary>
    /// 7 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet7")]
    public DataSetModel DetailDataSet7
    {
      get { return this.GetDataSetByIndex(7); }

      set { this.AddOrSetDataSet(7, value); }
    }

    /// <summary>
    /// 8 детальный раздел.
    /// </summary>
    [XmlElement("DetailDataSet8")]
    public DataSetModel DetailDataSet8
    {
      get { return this.GetDataSetByIndex(8); }

      set { this.AddOrSetDataSet(8, value); }
    }

    /// <summary>
    /// Добавить или установить набор данных.
    /// </summary>
    /// <param name="datasetIndex">Индекс набора данных.</param>
    /// <param name="dataset">Набор данных.</param>
    private void AddOrSetDataSet(int datasetIndex, DataSetModel dataset)
    {
      if (datasets.ContainsKey(datasetIndex))
        datasets[datasetIndex] = dataset;
      else
        datasets.Add(datasetIndex, dataset);
    }

    /// <summary>
    /// Получить набор данных по индексу.
    /// </summary>
    /// <param name="datasetIndex">Индекс набора данных.</param>
    /// <returns>Набор данных.</returns>
    public DataSetModel GetDataSetByIndex(int datasetIndex)
    {
      if (datasets.ContainsKey(datasetIndex))
        return datasets[datasetIndex];
      else
        return null;
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public DataSetsModel()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="model">Модель.</param>
    public DataSetsModel(DataSetsModel model)
    {
      if (model.DetailDataSet1 != null)
        this.DetailDataSet1 = new DataSetModel(model.DetailDataSet1);
      if (model.DetailDataSet2 != null)
        this.DetailDataSet2 = new DataSetModel(model.DetailDataSet2);
      if (model.DetailDataSet3 != null)
        this.DetailDataSet3 = new DataSetModel(model.DetailDataSet3);
      if (model.DetailDataSet4 != null)
        this.DetailDataSet4 = new DataSetModel(model.DetailDataSet4);
      if (model.DetailDataSet5 != null)
        this.DetailDataSet5 = new DataSetModel(model.DetailDataSet5);
      if (model.DetailDataSet6 != null)
        this.DetailDataSet6 = new DataSetModel(model.DetailDataSet6);
      if (model.DetailDataSet7 != null)
        this.DetailDataSet7 = new DataSetModel(model.DetailDataSet7);
      if (model.DetailDataSet8 != null)
        this.DetailDataSet8 = new DataSetModel(model.DetailDataSet8);
    }
  }
}
