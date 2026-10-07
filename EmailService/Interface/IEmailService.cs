using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Interface;

using EmailModel.Model;
public interface IEmailService
{
    Task SendEmail(EmailModel emailModel);
 }

