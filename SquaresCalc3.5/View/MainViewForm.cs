﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Shapes;

namespace SquaresCalc3._5.View
{
    /// <summary>
    /// Класс главной формы программы
    /// </summary>
    public partial class MainViewForm : Form
    {
        /// <summary>
        /// Свойство, содержащее ссылку на форму добалвения/изменения фигур
        /// </summary>
        public AddFigureForm AddFigureForm { get; }

        /// <summary>
        /// Свойство, содержащее ссылку на лист фигур
        /// </summary>
        public BindingList<IShape> Data { get; set; }


        /// <summary>
        /// Поле для хранения пути к открываемому файлу, переданному через командную строку в коструктор
        /// </summary>
        private readonly string _filePath = string.Empty;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public MainViewForm()
        {
            InitializeComponent();
            AddFigureForm = new AddFigureForm();
            Data = new BindingList<IShape>();
            shapesDataGridView.DataSource = Data;
            AddFigureForm.Data = Data;
            showShapePropertiesObjectControl.ControlsEnabled = false;

        }

        /// <summary>
        /// Конструктор с параметром
        /// </summary>
        /// <param name="filePath">Путь к открываемому файлу, переданному через командную строку</param>
        public MainViewForm(string filePath)
        {
            AddFigureForm = new AddFigureForm();
            InitializeComponent();
            showShapePropertiesObjectControl.ReadOnly = false;
            _filePath = filePath;
        }

        /// <summary>
        /// Обработчик клика по кнопке добавления фигуры
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void AddFigureButton_Click(object sender, EventArgs e)
        {

            if (!AddFigureForm.Visible)
            {
                AddFigureForm.SetAsAddForm();
                AddFigureForm.Show();
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления фигуры
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void RemoveFigureButton_Click(object sender, EventArgs e)
        {
            if (shapesDataGridView.SelectedRows.Count != 0)
            {
                foreach (DataGridViewRow item in shapesDataGridView.SelectedRows)
                {
                    shapesDataGridView.Rows.RemoveAt(item.Index);
                }
            }
            else
            {
                MessageBox.Show("Выберете строк(у|и) для удаления", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик изменения текста в текстбоксе поиска
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            shapesDataGridView.SelectionChanged -= ShapesDataGridView_SelectionChanged;
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                shapesDataGridView.DataSource = Data;
            }
            else
            {
                var tempBindingList = new BindingList<IShape>();
                foreach (IShape shape in Data)
                {
                    if (shape.Name.ToLower().Contains(SearchTextBox.Text.Trim().ToLower()))
                    {
                        tempBindingList.Add(shape);
                    }
                }
                shapesDataGridView.DataSource = tempBindingList;
            }
            shapesDataGridView.SelectionChanged += ShapesDataGridView_SelectionChanged;
        }

        /// <summary>
        /// Обработчик клика по кнопке сохранения
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void SaveDataButton_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                FileName = "SquareCalcData",
                DefaultExt = "scl",
                Filter = "SquareCal3.5 Files | *.scl"
            };
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != String.Empty)
            {
                Serialize(saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке загрузки
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void LoadDataButton_Click(object sender, EventArgs e)
        {
            shapesDataGridView.SelectionChanged -= ShapesDataGridView_SelectionChanged;
            var openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != string.Empty)
            {
                Deserialize(openFileDialog.FileName);
            }
            shapesDataGridView.SelectionChanged += ShapesDataGridView_SelectionChanged;
        }

        /// <summary>
        /// Сериализация
        /// </summary>
        /// <param name="filePath">Путь к сериализуемому файлу</param>
        private void Serialize(string filePath)
        {
            if (Data.Count != 0)
            {
                string jsonContents = JsonConvert.SerializeObject(AddFigureForm.Data, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    using (TextWriter tw = new StreamWriter(fs))
                    {
                        tw.Write(jsonContents);
                    }
                }
            }
        }

        /// <summary>
        /// Десериализация
        /// </summary>
        /// <param name="filePath">Путь к десериализуемому файлу</param>
        private void Deserialize(string filePath)
        {
            shapesDataGridView.SelectionChanged -= ShapesDataGridView_SelectionChanged;
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (TextReader tw = new StreamReader(fs))
                {
                    var jsonContents = tw.ReadToEnd();
                    AddFigureForm.Data = JsonConvert.DeserializeObject<BindingList<IShape>>(jsonContents,
                        new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });
                }
            }
            shapesDataGridView.DataSource = Data;
            shapesDataGridView.SelectionChanged += ShapesDataGridView_SelectionChanged;
        }

        /// <summary>
        /// Обработчик клика в текстбоксе поиска
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void SearchTextBox_Click(object sender, EventArgs e)
        {
            if (!AddFigureForm.Visible)
            {
                AddFigureForm.Hide();
            }
        }

        /// <summary>
        /// Обработчик изменения выбора в таблице с данными
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void ShapesDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            showShapePropertiesObjectControl.ShapesDataShow(Data[shapesDataGridView.SelectedRows[0].Index]);
            showShapePropertiesObjectControl.ReadOnly = true;
        }

        /// <summary>
        /// Обработчик клика кнопки изменения фигуры
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void CallModifyFigureFormButton_Click(object sender, EventArgs e)
        {
            AddFigureForm.SetAsModifyForm(
                (shapesDataGridView.DataSource as BindingList<IShape>)[shapesDataGridView.SelectedRows[0].Index],
                shapesDataGridView.SelectedRows[0].Index);
            AddFigureForm.Show();
        }

        /// <summary>
        /// Обработчик загрузки формы
        /// </summary>
        /// <param name="sender">Ссылка на объект-отправитель</param>
        /// <param name="e">Параметры события</param>
        private void MainViewForm_Load(object sender, EventArgs e)
        {
            if (_filePath != string.Empty)
            {
                Deserialize(_filePath);
            }
        }
    }
}
