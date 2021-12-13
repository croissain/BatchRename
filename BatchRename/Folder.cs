using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename
{
    public class Folder : INotifyPropertyChanged
    {
        private string _folder;
        private string _newfolder;
        private string _path;
        private string _status;

        public string Foldername
        {
            get => _folder; set
            {
                _folder = value;
                NotifyChanged("Folder");
            }
        }

        public string Newfolder
        {
            get => _newfolder;
            set
            {
                _newfolder = value;
                NotifyChanged("Newfolder");
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                NotifyChanged("Path");
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                NotifyChanged("Status");
            }
        }

        private void NotifyChanged(string v)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(v));
            }
        }

        public Folder Clone()
        {
            return new Folder()
            {
                Foldername = this._folder,
                Newfolder = this._newfolder,
                Path = this._path,
                Status = this._status
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
