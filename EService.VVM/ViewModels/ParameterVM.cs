﻿using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EService.VVM.ViewModels
{
    class ParameterVM : INotifyPropertyChanged
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter; //Параметр для формирования лямбды фильтрации     

        private PView _selectedParameter; //Выбранный параметр
        private Model _selectedModel; //Выбранная модель
        private ParameterForModel _selectedParameterForModel; //Выбранный параметр модели

        private IList<Model> _selectedModels; //Список выбранных моделей устройств
        private IList<ParameterForModel> _selectedParameterForModels; //Список выбранных моделей устройств

        private IList<Model> _models; //Список моделей устройств
        private IList<ParameterForModel> _parameterForModels; //Список моделей устройств

        private IDelegateCommand _openAddTypeModelWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand _openEditTypeModelWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand _refreshTypeModelWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        public PView SelectedParameter 
        { 
            get 
            { 
                return _selectedParameter; 
            } 
            set 
            { 
                _selectedParameter = value;
                if (_dbContext is SQLiteContext)
                {
                    SQLiteContext context = _dbContext as SQLiteContext;
                    if (SelectedParameter != null)
                    {
                        context.ParameterForModel.Load();
                        ParameterForModels = context.ParameterForModel.Where(s => s.Parameter.Rowid == SelectedParameter.Parameter.Rowid).ToList();
                        context.Model.Load();
                        Models = context.Model.Local.Where(s => ParameterForModels.All(pfm => pfm.Model.Rowid != s.Rowid)).ToList();
                    }
                }                
                OnPropertyChanged("SelectedParameter"); 
            } 
        }
        public Model SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public ParameterForModel SelectedParameterForModel { get { return _selectedParameterForModel; } set { _selectedParameterForModel = value; OnPropertyChanged("SelectedParameterForModel"); } }
        public String Search
        {
            get { return _search; }
            set
            {
                _search = value;
                _filterSearch.SetWhat(_search); // Задание поисковой строки
                _filterSearch.SetWhere("Name"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public IList<PView> Parameters { get; set; }
        public IList<Model> Models { get { return _models; } set { _models = value; OnPropertyChanged("Models"); } }
        public IList<ParameterForModel> ParameterForModels { get { return _parameterForModels; } set { _parameterForModels = value; OnPropertyChanged("ParameterForModels"); } }
        public ObservableCollection<Model> SelectedModels
        {
            get { return (ObservableCollection<Model>)_selectedModels; }
            set
            {
                _selectedModels = value;
                OnPropertyChanged("SelectedModels");
            }
        }
        public ObservableCollection<ParameterForModel> SelectedParameterForModels
        {
            get { return (ObservableCollection<ParameterForModel>)_selectedParameterForModels; }
            set
            {
                _selectedParameterForModels = value;
                OnPropertyChanged("SelectedParameterForModels");
            }
        }
        #endregion

        #region Конструктор
        public ParameterVM()
        {
            InitializeFilters();
            Parameters = new ObservableCollection<PView>();
            SelectedModels = new ObservableCollection<Model>();
            SelectedParameterForModels = new ObservableCollection<ParameterForModel>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;                
                context.Parameter.Load();
                var parametersList = context.Parameter.Local.ToBindingList();
                ParametersListCreator(parametersList);
            }

        }
        #endregion

        #region Методы
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Parameter), "s");
            _filterSearch = new FilterSearch(_parameter);

            _filters = new List<IFilter>();

            _filters.Add(_filterSearch);

            _filterSearch.FilterCreated += OnFilterChanged;
        }

        private void SetFilter<T>(ObservableCollection<T> list, IFilter filter, params string[] parameters) where T : IIdentifier
        {
            List<string> indeses = new List<string>();
            foreach (var item in list)
            {
                indeses.Add(item.Rowid.ToString());
            }
            filter.SetWhat(indeses.ToArray());
            filter.SetWhere(parameters);
            filter.CreateFilter();
        }
        private void ParametersListCreator(IList<Parameter> list)
        {
            Parameters.Clear();
            foreach (var item in list)
            {
                Parameters.Add(new PView(item, Parameters.Count() + 1));
            }
        }

        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedParameterForModels, typeof(ParameterForModel), SelectedParameterForModel);
                if (temp != null) SelectedParameterForModels = (ObservableCollection<ParameterForModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedModels, typeof(Model), SelectedModel);
                if (temp != null) SelectedModels = (ObservableCollection<Model>)temp;

                OnFilterChanged();
            }
        }

        public void OnFilterChanged()
        {
            IList<Parameter> tempParameters = new ObservableCollection<Parameter>();
            System.Linq.Expressions.Expression result = null, temp;
            Delegate lambda = null;
            foreach (var item in _filters)
            {
                if (result == null)
                    result = item.GetFilter();
                else
                {
                    temp = item.GetFilter();
                    if (temp != null)
                        result = System.Linq.Expressions.Expression.And(result, temp);
                }
            }
            if (result != null)
            {
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Parameter, bool>>(result, _parameter).Compile();
            }
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempParameters = context.Parameter.ToList();
                if (lambda != null)
                    tempParameters = context.Parameter.Where((Func<Parameter, bool>)lambda).ToList();
                ParametersListCreator(tempParameters);
            }
            SelectedParameter = null;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public class PView
        {
            public Parameter Parameter { get; private set; }
            public int Index { get; set; }

            public PView(Parameter parameter, int index)
            {
                Parameter = parameter;
                Index = index;
            }
        }
    }
}
