using Microsoft.EntityFrameworkCore;
using PhoneBook.Models;
using PhoneBookWpf;
using PhoneBookWpf.Data;
using PhoneBookWpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PhoneBook.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IDialogService _dialogs;
        private readonly PhoneBookContext _context;

        public ObservableCollection<Contact> Contacts { get; }

        #region Свойства для полей ввода

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

        #endregion

        #region Команды

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        #endregion

        #region Конструктор

        public MainViewModel(IDialogService dialogs, PhoneBookContext context)
        {
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            // Загружаем данные из БД в локальный кэш
            _context.Contacts.Load();

            // Привязываем коллекцию к локальному кэшу EF Core
            Contacts = _context.Contacts.Local.ToObservableCollection();

            // Инициализация команд
            AddCommand = new RelayCommand(AddContact, CanAddContact);
            EditCommand = new RelayCommand<Contact>(EditContact, CanEditOrDelete);
            DeleteCommand = new RelayCommand<Contact>(DeleteContact, CanEditOrDelete);
        }

        #endregion

        #region Методы команд

        // ─── CREATE ──────────────────────────────────────────────────────────

        private void AddContact()
        {
            // Валидация: имя не пустое, телефон — 10 цифр или +7 + 10 цифр
            if (string.IsNullOrWhiteSpace(Name) ||
                !Regex.IsMatch(Phone, @"^(\+7)?\d{10}$"))
            {
                _dialogs.ShowError(
                    "Некорректные значения.\n" +
                    "Телефон должен быть в формате +7XXXXXXXXXX или 10 цифр.");
                return;
            }

            // Проверка дубликата по номеру телефона
            if (Contacts.Any(c => Normalize(c.Phone) == Normalize(Phone)))
            {
                _dialogs.ShowWarning("Контакт с таким номером уже существует!");
                return;
            }

            try
            {
                var contact = new Contact
                {
                    Name = Name.Trim(),
                    Phone = Phone.Trim()
                };

                // Помечаем как Added → SaveChanges сгенерирует INSERT
                _context.Contacts.Add(contact);
                _context.SaveChanges();

                _dialogs.ShowInfo("Контакт успешно добавлен.");

                // Сбрасываем поля ввода
                Name = string.Empty;
                Phone = string.Empty;
            }
            catch (Exception ex)
            {
                _dialogs.ShowError($"Ошибка при добавлении контакта:\n{ex.Message}");
            }
        }

        private bool CanAddContact() =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Phone);

        // ─── UPDATE ──────────────────────────────────────────────────────────

        private void EditContact(Contact? contact)
        {
            if (contact is null) return;

            // Открываем окно редактирования с текущими данными контакта
            var window = new EditContactWindow(contact.Name, contact.Phone)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Если пользователь нажал "Сохранить"
            if (window.ShowDialog() == true)
            {
                // Валидация нового телефона
                if (!Regex.IsMatch(window.ContactPhone, @"^(\+7)?\d{10}$"))
                {
                    _dialogs.ShowError(
                        "Некорректный номер телефона.\n" +
                        "Формат: +7XXXXXXXXXX или 10 цифр.");
                    return;
                }

                // Проверка дубликата (исключаем сам редактируемый контакт)
                if (Contacts.Any(c => c.Id != contact.Id &&
                                      Normalize(c.Phone) == Normalize(window.ContactPhone)))
                {
                    _dialogs.ShowWarning("Контакт с таким номером уже существует!");
                    return;
                }

                try
                {
                    // Объект уже отслеживается контекстом —
                    // просто меняем свойства, EF Core сам сделает UPDATE
                    contact.Name = window.ContactName;
                    contact.Phone = window.ContactPhone;

                    _context.SaveChanges();

                    _dialogs.ShowInfo("Контакт успешно обновлён.");
                }
                catch (Exception ex)
                {
                    _dialogs.ShowError($"Ошибка при сохранении изменений:\n{ex.Message}");
                }
            }
        }

        // ─── DELETE ──────────────────────────────────────────────────────────

        private void DeleteContact(Contact? contact)
        {
            if (contact is null) return;

            if (!_dialogs.ShowConfirmation($"Удалить контакт «{contact.Name}»?"))
                return;

            try
            {
                // Помечаем как Deleted → SaveChanges сгенерирует DELETE
                _context.Contacts.Remove(contact);
                _context.SaveChanges();

                // UI обновится автоматически через Local.ToObservableCollection()
            }
            catch (Exception ex)
            {
                _dialogs.ShowError($"Ошибка при удалении контакта:\n{ex.Message}");
            }
        }

        // ─── CanExecute ───────────────────────────────────────────────────────

        private bool CanEditOrDelete(Contact? contact) => contact is not null;

        #endregion

        #region Вспомогательные методы

        // Нормализация телефона: оставляем только цифры для сравнения
        private static string Normalize(string? phone) =>
            new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());

        #endregion
    }
}