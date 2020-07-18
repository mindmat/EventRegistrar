using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using EventRegistrar.OrganizerApp.Models;
using EventRegistrar.OrganizerApp.Services;
using EventRegistrar.OrganizerApp.Views;

using Xamarin.Forms;

namespace EventRegistrar.OrganizerApp.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public ObservableCollection<Item> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command LoginCommand { get; set; }
        private readonly SocialAuthenticator _authenticator;
        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            _authenticator = new SocialAuthenticator();

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            LoginCommand = new Command(async () => await Login());



            MessagingCenter.Subscribe<NewItemPage, Item>(this, "AddItem", async (obj, item) =>
            {
                var newItem = item as Item;
                Items.Add(newItem);
                await DataStore.AddItemAsync(newItem);
            });
        }

        private async Task Login()
        {
            await _authenticator.Authenticate();
        }

        private async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}