using System;
using DataLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;

namespace Services
{
    public delegate void AddCurrentUserDelegate(int count); //Вказівник на функцію яка приймає ціле число і нічого не повертає
    /// <summary>
    /// Керування користувачами
    /// </summary>
    public class UserService
    {
        public event AddCurrentUserDelegate AddCurrentUserEvent;  // Подія яка спрацьовує при додавані користувача. На цей івент можна підписатись, і ми будемо перевіряти чи хтось підписався
        public UserService()
        {
            MyDataContext myDataContext = new MyDataContext();
        }
        public Task InsertRandomUserAsync(int count)
        {
            return Task.Run(() => InsertRandomUser(count));
        }

        //Додавання рандомних даних в БД
        public void InsertRandomUser(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(100);
                if (AddCurrentUserEvent != null) // якщо на цей івент хтось підписався
                    AddCurrentUserEvent(i + 1); // то ми в цей івент передаємо кількість елементів - які ми додали 
            }
        }
    }
}
