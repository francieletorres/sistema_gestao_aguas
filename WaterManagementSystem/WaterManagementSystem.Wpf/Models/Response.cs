using System;
using System.Collections.Generic;
using System.Text;

namespace WaterManagementSystem.Wpf.Models
{
    public class Response
    {
        public bool IsSuccess { get; set; } //para verificar se tem internet ou n, se a api carregou etc

        public string Message { get; set; } //caso corra mal, mostra a mensagem

        public object Result { get; set; } // se correr tudo bem guarda o objet

    }
}
