using System;
using DataLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using System.Net;
using DataLibrary.Entities;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Services
{
    public delegate void AddCurrentUserDelegate(int count, ManualResetEvent manualResetEvent, CancellationToken token); //Вказівник на функцію яка приймає ціле число і нічого не повертає
    /// <summary>
    /// Керування користувачами
    /// </summary>
    public class UserService
    {
        private object thisLock = new object();
        public event AddCurrentUserDelegate AddCurrentUserEvent;  // Подія яка спрацьовує при додавані користувача. На цей івент можна підписатись, і ми будемо перевіряти чи хтось підписався
        private MyDataContext myDataContext;
        public UserService()
        {
            myDataContext = new MyDataContext();
        }

        public Task InsertRandomUserAsync(int count, ManualResetEvent manualResetEvent, CancellationToken token)
        {
            return Task.Run(() => InsertRandomUser(count, manualResetEvent, token));
        }

        //Додавання рандомних даних в БД
        public void InsertRandomUser(int count, ManualResetEvent manualResetEvent, CancellationToken token)
        {
            //for (int i = 0; i < count; i++)
            //{
            //    Thread.Sleep(100);
            //    if (AddCurrentUserEvent != null) // якщо на цей івент хтось підписався
            //        AddCurrentUserEvent(i + 1); // то ми в цей івент передаємо кількість елементів - які ми додали 
            //}
            lock (thisLock)
            {
                Faker<User> faker = new Faker<User>()
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.Phone, (f, u) => f.Phone.PhoneNumber())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email());
                for (int i = 0; i < count; i++)
                {
                    if (AddCurrentUserEvent != null)
                    {
                        manualResetEvent.WaitOne(Timeout.Infinite);
                        Thread.Sleep(500);
                        if (token.IsCancellationRequested)
                        {
                            myDataContext.SaveChanges();
                            AddCurrentUserEvent(i, manualResetEvent, token);
                            return;
                        }
                        User user = faker.Generate();
                        myDataContext.Users.Add(user);
                        AddCurrentUserEvent(i + 1, manualResetEvent, token);
                    }
                }
                myDataContext.SaveChanges();
            }
        }
    }
}
