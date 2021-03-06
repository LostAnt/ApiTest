﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using apiTest.Entities;

namespace apiTest.Models
{
    public class RegisterModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        public string Patronymic { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
