using PhoneBook.Models;
using PhoneBookWpf.Data;
using PhoneBookWpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace PhoneBook.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IDialogService _dialogs;
        private readonly PhoneBookContext _context;

        public ObservableCollection<Contact> Contacts { get; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (Set(ref _name, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set
            {
                if (Set(ref _phone, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        private Contact? _selectedContact;
        public Contact? SelectedContact
        {
            get => _selectedContact;
            set
            {
                if (Set(ref _selectedContact, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainViewModel(IDialogService dialogs, PhoneBookContext context)
        {
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _context.Contacts.Load();
            Contacts = _context.Contacts.Local.ToObservableCollection();

            AddCommand = new RelayCommand(AddContact, CanAddContact);
            DeleteCommand = new RelayCommand<Contact>(DeleteContact, CanDeleteContact);
        }

        private void AddContact()
        {
            // Проверка на валидность перед добавлением
            if (string.IsNullOrWhiteSpace(Name) || !Regex.IsMatch(Phone, @"^(\+7)?\d{10}$"))
            {
                _dialogs.ShowError("Некорректные значения имени или телефона. Телефон должен быть в формате +7... или 10 цифр без +7.");
                return;
            }

            if (Contacts.Any(c => Normalize(c.Phone) == Normalize(Phone)))
            {
                _dialogs.ShowWarning("Контакт с таким номером уже существует!");
                return;
            }

            var contact = new Contact { Name = this.Name, Phone = this.Phone };

            _context.Contacts.Add(contact);
            _context.SaveChanges();

            _dialogs.ShowInfo("Контакт успешно добавлен.");
            Name = string.Empty;
            Phone = string.Empty;
        }

        private bool CanAddContact() =>
            !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Phone);

        private void DeleteContact(Contact? contact)
        {
            if (contact is null) return;

            if (!_dialogs.ShowConfirmation($"Удалить контакт «{contact.Name}»?"))
                return;

            _context.Contacts.Remove(contact);
            _context.SaveChanges();
        }

        private bool CanDeleteContact(Contact? contact) => contact != null;

        private static string Normalize(string? phone) =>
            new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
    }
}