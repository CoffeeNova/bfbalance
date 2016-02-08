//      *********    НЕ ИЗМЕНЯЙТЕ ЭТОТ ФАЙЛ     *********
//      Этот файл обновляется средством разработки. Внесение
//      изменений в этот файл может привести к ошибкам.
namespace Expression.Blend.DataStore.DataStore1
{
	using System;
	using System.Collections.Generic;

	public class DataStore1GlobalStorage
	{
		public static DataStore1GlobalStorage Singleton;
		public bool Loading {get;set;}
		private List<WeakReference> registrar; 

		public DataStore1GlobalStorage()
		{
			this.registrar = new List<WeakReference>();
		}
		
		static DataStore1GlobalStorage()
		{
			Singleton = new DataStore1GlobalStorage();
		}

		public void Register(DataStore1 dataStore)
		{
			this.registrar.Add(new WeakReference(dataStore));
		}

		public void OnPropertyChanged(string property)
		{
			foreach (WeakReference entry in this.registrar)
			{
				if (!entry.IsAlive)
				{
					continue;
				}
				DataStore1 dataStore = (DataStore1)entry.Target;
				dataStore.FirePropertyChanged(property);
			}
		}
		
		public bool AssignementAllowed
		{
			get
			{
				// Назначать только при однократной загрузке
				if(this.Loading && this.registrar.Count > 0)
				{
					return false;
				}
				
				return true;
			}
		}
	}

	public class DataStore1 : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		public void FirePropertyChanged(string propertyName)
		{
			this.OnPropertyChanged(propertyName);
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}

		public DataStore1()
		{
			try
			{
				System.Uri resourceUri = new System.Uri("/ByflyView;component/DataStore/DataStore1/DataStore1.xaml", System.UriKind.Relative);
				if (System.Windows.Application.GetResourceStream(resourceUri) != null)
				{
					DataStore1GlobalStorage.Singleton.Loading = true;
					System.Windows.Application.LoadComponent(this, resourceUri);
					DataStore1GlobalStorage.Singleton.Loading = false;
					DataStore1GlobalStorage.Singleton.Register(this);
				}
			}
			catch (System.Exception)
			{
			}
		}
	}
}
