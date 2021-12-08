using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
namespace AutoLoginFbKaraokeLd.UI.ViewModels
{
    class NameAttribute : Attribute
    {
        public NameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
    enum MenuAction
    {
        [Name("Thêm tài khoản")]
        ImportAccount,
    }


    internal class MenuVM
    {
        public MenuVM(MenuAction action)
        {
            this.Action = action;
            this.Title = action.GetAttribute<NameAttribute>().Name;
        }
        public MenuAction Action { get; }
        public string Title { get; }
    }
}
