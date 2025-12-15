using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace MauiApp4.ViewModels
{
    public partial class CustomerEditViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        public partial Customer Item { get; set; }

        public Func<Customer, Task>? ParentRefreshAction { get; set; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Item", out object currentItem))
            {
                Item = (Customer)currentItem;
            }
            if (query.TryGetValue("ParentRefreshAction", out object parentRefreshAction))
            {
                ParentRefreshAction = (Func<Customer, Task>)parentRefreshAction;
            }
            query.Clear();
        }

        [RelayCommand]
        async Task SaveAsync()
        {
            using (var context = new CrmContext())
            {
                if (Item.Id == 0)
                    context.Customers.Add(Item);
                else
                    context.Customers.Update(Item);

                await context.SaveChangesAsync();
            }
            
            await ParentRefreshAction(Item);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }


    }
}
