using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanWidth
{
    class ConsoleTable<T>
        where T : class, INotifyPropertyChanged
    {
        public ICollection<ConsoleColumn<T>> Columns { get; private set; }

        public ICollection<T> Rows { get; set; }
        private IDictionary<T, ConsoleLine> _lines;

        private ConsoleLine _header;

        public ConsoleTable()
        {
            _header = new ConsoleLine();

            var columns = new ObservableCollection<ConsoleColumn<T>>();
            Columns = columns;

            columns.CollectionChanged += (sender, args) =>
            {
                UpdateLine(_header, Columns.Select(c => c.Header).ToArray());
            };

            _lines = new Dictionary<T, ConsoleLine>();

            var rows = new ObservableCollection<T>();
            Rows = rows;
            rows.CollectionChanged += (sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:

                        // TODO: reuse console lines after removal
                        foreach (T item in args.NewItems)
                        {
                            if (!_lines.ContainsKey(item))
                            {
                                _lines.Add(item, new ConsoleLine());
                                item.PropertyChanged += ItemOnPropertyChanged;
                                UpdateRow(item);
                            }
                        }
                        break;
                }
            };
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // TODO: faster algorithm
            T item = sender as T;
            if (item != null)
            {
                UpdateRow(item);
            }
        }

        private void UpdateRow(T row)
        {
            ConsoleLine line = _lines[row];
            object[] props = Columns.Select(c => c.Property(row) ?? "...").ToArray();
            
            UpdateLine(line, props);
        }

        private void UpdateLine(ConsoleLine line, object[] columns)
        {
            if (columns.Length != Columns.Count)
            {
                throw new Exception("Columns do not match");
            }

            UpdateLine(line, Columns.Zip(columns, (c, o) => String.Format(c.Format, o)).ToArray());
        }

        private void UpdateLine(ConsoleLine line, string[] columns)
        {
            if (columns.Length != Columns.Count)
            {
                throw new Exception("Columns do not match");
            }
            
            line.Write(String.Join(" ", Columns.Zip(columns, (c, str) => str.PadRight(c.Width).Substring(0, c.Width))));
        }
    }

    class ConsoleColumn<T>
    {
        public string Header { get; private set; } // TODO: react to change

        public string Format { get; private set; } // TODO: react to change

        public Func<T, object> Property { get; private set; }

        public int Width { get; private set; } // TODO: react to change

        public ConsoleColumn(string header, string format, int width, Func<T, object> prop)
        {
            this.Header = header;
            this.Format = format;
            this.Width = width;
            this.Property = prop;
        } 
    }
}
