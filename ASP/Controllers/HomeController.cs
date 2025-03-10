using ASP.Data;
using ASP.Models;
using ASP.Models.FrontendForm;
using ASP.Models.Home.Ioc;
using ASP.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ASP.Models.Home.Signup;
using System.Text.RegularExpressions;
using ASP.Data.DAL;
using ASP.Services.Kdf;
using ASP.Services.Email;
using System.Net.Mail;
using ASP.Models.Home.Signup.MailTemplates;

namespace ASP.Controllers
{
    public class HomeController : Controller
	{
		/* �������� ������ (�����������) - ����� � ����������
        �� �������� �������� �� �������� ��'����. �������
        �������������� ����� �������� - ����� �����������. ��
        ��������, ��-�����, ��������� ���� �� ������� (readonly) ��,
        ��-�����, ������������ ��������� ��'���� ��� ��������
        �����������. � ���������� ����� �������� �������������
        �� ����� ��������� (_logger)
        */

		// �������� ��������� ����� - ���� � �� ������, �� ���� ������
		private readonly DataContext _dataContext;
		private readonly DataAccessor _dataAccessor;
		private readonly IKdfService _kdfService;
		private readonly IEmailService _emailService;

		private readonly ILogger<HomeController> _logger;

		// ��������� ���� ��� ��������� �� �����
		private readonly IHashService _hashService;

		//������ �� ������������ ��������-���������� � �������� �� � ��
		public HomeController(IHashService hashService, ILogger<HomeController> logger, DataContext dataContext, DataAccessor dataAccessor, IKdfService kdfService, IEmailService emailService)
		{
			_logger = logger;               // ���������� ��������� �����������, �� ��
			_hashService = hashService;     // ������ ��������� ��� ��������� ����������
			_dataContext = dataContext;
			_dataAccessor = dataAccessor;
			_kdfService = kdfService;
			_emailService = emailService;
		}
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult ConfirmEmail(String Id)
		{
			/*	���� Basic-�������������� - ��������� ����� �� ������
			 *	����� ":" �� ��������� � Base64
			 *	user@i.ua:123 --- dXNlckBpLnVh0jEyMw ==
			 */
			String email, code;
			try
			{
				String data = System.Text.Encoding.UTF8.GetString(
				Convert.FromBase64String(Id));          // user@i.ua:123
				String[] parts = data.Split(':', 2);    // [user@i.ua, 123]
				email = parts[0];                       // user@i.ua
				code = parts[1];                        // 123
				ViewData["result"] = _dataAccessor.UserDao.ConfirmEmail(email, code)
					? "����� ������ �����������"
					: "������� ������������ �����";
			}
			catch
			{
				ViewData["result"] = "���� �� ���������";
			}
			return View();
		}
		public IActionResult Intro()
		{
			return View();
		}
		public IActionResult AboutRazor()
		{
			Models.Home.AboutRazor.AboutRazorPageModel aboutRazorModel = new()
			{
				TabHeader = "Razor",
				PageTitle = "��������� Razor",
				RazorIs = "��������� ��������� �� ������ HTML �������\r\n\t������ ���� ������������� C#.",
				RazorInstrc = "���������� - ����������� ������ ���� �������������, �� ������ ����\r\n\t�� �������� " +
				"(���������� ���� �� ������� ��������),\r\n\t��� � ���������������� ���������.\r\n\tRazor-i��������� ������� � ������� ����� " +
				"&commat;{...}\r\n\t� �������� ����� ������ ���� ������� ���������� ����� �#.\r\n\t������������� ������� ���� � ����������, ��" +
				" ���������� ��������\r\n\t�� ������������� ���� � �� " +
				"�������������� ������ �� ���� ������\r\n\t������.",
			};
			return View(aboutRazorModel);
		}
		// ������ ����� ����������� ���������� ������, ����������-�����������
		public IActionResult Model(Models.Home.Model.FormModel? formModel)
		{
			// ������ ������������� ����������� � ������������ ������i���
			Models.Home.Model.PageModel pageModel = new()
			{
				TabHeader = "�����i",
				PageTitle = "�����i � ASP",
				FormModel = formModel
			};
			// ������ ������������� ���������� ���������� View()
			return View(pageModel);
		}

		[HttpPost]      //������� �������� ����� ���� POST-�������
		public JsonResult FrontendFrom([FromBody] FrontendFormInput input)
		{
			FrontendFormOutput output = new()
			{
				Code = 200,
				//Message = $"{input.UserName} -- {input.UserEmail} -- {input.UserGen} -- {input.UserDate.ToString().Substring(0, 10)}"
			};
			_logger.LogInformation(output.Message);
			return Json(output);
		}

		public IActionResult Data()
		{
			Models.Home.Data.DataPageModel dataPageModel = new()
			{
				TabHeader = "������ � ��",
				PageTitle = "������ � ������. �i��������� ��",
				NuGetPackages = new List<String>
				{
					"Microsoft.EntityFrameworkCore - ���� ����������, ������� ������",
					"Microsoft. EntityFrameworkCore.Tools - ����������� ��������� ���������",
					"������� ��: � ��������� �� ���� -\r\n\t\t��� MSSQL: " +
					"Microsoft.EntityFrameworkCore.SqlServer\r\n\t\tMySQL: Pomelo.EntityFrameworkCore.MySql"
				},
				DataStruct = new List<String>
				{
					"��������� � ������ ������ ����� Data, � ��� - ���� DataContext",
					"�������� ����� ���������� �� ��. MSSQL ���� ���������� ��,\r\n\t\t�������� ����� �������� ����� �� ���� �� �������� ��.\r\n\t\tMySQL - ����� �������� ��, ��� �������� ���������. ����� ����������\r\n\t\t������� �� appsettings.json � ���������� ������ \"ConnectionStrings\"",
					"��������� ����� Data/Entities, � ��� ���� - User",
					"���� DataContext ������������� �� ����� DbContext. � ���� DataContext ������� ������������� ������ \r\n\t\tOnConfiguring() � OnModelCreating(), � ���� ����������� ��������� �������� DbContextOptions ����� base(options).",
					"� ���� Program.cs ��������� ������ �������� ����� �� ��������� builder.Services.AddDbContext<DataContext>\r\n\t\t\t\t\t(options =>\r\n\t\t\t\t\toptions.UseSqlServer(\r\n\t\t\t\t\tbuilder.Configuration.GetConnectionString(\"LocalMSSQL\")));",
					"���� ����� ��������������� ������� add-migration '�������� ��������' � ������� ��� ��������� �������.",
					"ϳ��� ��������� ������� �� ����� ����������� �� ���� ����� �� ��������� ������� update-database."
				}
			};
			ViewData["users-count"] = _dataContext.Users.Count();
			return View(dataPageModel);
		}
		public IActionResult Ioc(String? format)  // Inversion of Control
		{
			// ����������� ������������ �������
			// ViewData["hash"] = 
			// ViewData - ����������� ��'��� ��� �������� �����
			// �� �������������. ���� ����� �� ������ ["hash"]
			// ����� ���������� � ��������� �������
			IocPageModel pageModel = new()
			{
				TabHeader = "IoC",
				PageTitle = "������i� ���������. ������. ",
				SingleHash = _hashService.Digest("123"),
				IoCIs = "IoC (Inversion of Control, ������� ���������) - ������������� ������," +
				"\r\n\t����� � ���� ������ ��������� ������� ������ ��'���� ��������������" +
				"\r\n\t�� ����������� ������ (��������, ��������� �����������, ���������)." +
				"\r\n\t������� ���� ��'����: CRUD. ��������� �� ������, �� ������ ���������" +
				"\r\n\tnew / delete ������ �������� ��������� �� ����������.",
				IoCOptions = new List<String>
				{
					"��������� ������ - �����, �� ���� ��������� ����������������. ",
					"��������� ��� ������ � ��������� (��������)",
					"�������� ������ � ���� ��'����, ���� ���� �������"
				},
				HashExm = new List<String>
				{
					"(����������) ��������� ����� Services � ������ ������. ",
					"������� ����� - �� ���������� ��� ����� (���� �� ���������),\r\n\t\t��� ������� ������ ����� ����������� ����� (Hash)",
					"��������� ��������� IHashService �� ���� Md5HashService ",
					"P������� ����� (���. Program.cs, ����� 8 � ���)",
					"��������� ����� (���. HomeController)",
					$"���������� ���� ������: {_hashService.Digest("123")}",
					"I����� ������: ��������� ������� �� ����� ���-�������� SHA",
					"OCP (3 SOLID) \"��������, ��� �� �����\" -- ��������� �����\r\n\t\t���� ShaHashService � ����� Services/Hash"
				},
				Title = "������� ���������� ���������:",
			};
			for (int i = 0; i < 5; i++)
			{
				String str = (i + 100500).ToString();
				pageModel.Hashes[str] = _hashService.Digest(str);
			}
			if (format == "json")
				return Json(pageModel);
			return View(pageModel);
		}
		public IActionResult URLStruct()
		{
			Models.Home.URLStruct.URLStructPageModel uRLStructPage = new()
			{
				TabHeader = "URL",
				PageTitle = "��������� URL",
				PageText = new List<String>
				{
					"��������: This is the designation of the protocol that is used to access the resource. \r\n        For example, http://, https://, ftp://, mailto:, etc..",
					"�����: This is the part of the URL that indicates the address of the server on which the resource is hosted. \r\n        For example, www.example.com.",
					"����: This points to a specific path to a resource on the server. This is the part of the URL after the domain. \r\n        For example, /path/to/resource.",
					"������ �������: These are the parameters passed in the URL for a request to a resource. \r\n        They begin with a question symbol ? and can contain key-value pairs separated by the ampersand & character. For example, ?key1=value1&key2=value2.",
					"��������: This is a specific part of the resource that you need to go to or scroll to after the page has loaded. \r\n        The fragment begins with a hash symbol #. For example, #section1."
				},
				PageImageSrc = "/img/url.jpg"
			};
			return View(uRLStructPage);
		}

		public IActionResult Privacy()
		{
			Models.Home.Privacy.PrivacyPageModel privacyPage = new()
			{
				TabHeader = "Privacy",
				PageTitle = "Privacy Policy",
				PageText = "1. Introduction\n\nTech Solutions Inc. (\"we,\" \"our,\" or \"us\") is committed to protecting your privacy. " +
				"This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you visit our website www.techsolutionsinc.com, " +
				"use our services, or otherwise interact with us. Please read this policy carefully to understand our views and practices regarding your personal data " +
				"and how we will treat it.\n\n2. Information We Collect\n\nWe may collect and process the following types of information:\n\n2.1 Personal Information:\n\nName\nEmail address\nPhone number\nAddress\nPayment information\n2.2 Non-Personal " +
				"Information:\n\nBrowser type\nOperating system\nIP address\nPages visited on our website\nTime and date of visit\n3. How We Use Your Information\n\nWe use the information we collect for the " +
				"following purposes:\n\nTo provide, operate, and maintain our services\nTo improve, personalize, and expand our services\nTo communicate with you, including customer service and support\nTo process transactions and send related " +
				"information\nTo send promotional materials and updates\nTo detect and prevent fraud\nTo comply with legal obligations\n4. How We Share Your Information\n\nWe do not sell, trade, or otherwise transfer your personal " +
				"information to outside parties except in the following circumstances:\n\nWith your consent\nWith service providers who perform services on our behalf\nTo comply with legal obligations\nTo protect and defend our rights and property" +
				"\nIn connection with a business transfer (e.g., merger, acquisition)\n5. Data Security\n\nWe implement a variety of security measures to maintain the safety of your personal information." +
				" However, no method of transmission over the Internet or electronic storage is 100% secure. " +
				"Therefore, while we strive to use commercially acceptable means to protect your personal information, we cannot guarantee its absolute security.."

			};
			return View(privacyPage);
		}
		public IActionResult Signup(SingupFormModel? formModel)
		{
			SingupPageModel pageModel = new()
			{
				FormModel = formModel
			};
			if (formModel?.HasData ?? false)
			{
				pageModel.ValidationErrors = _ValidateSingupModel(formModel);
				String code = RandomStringService.GenerateOTP(6);
				String slug = Convert.ToBase64String(
					System.Text.Encoding.UTF8.GetBytes(
					$"{formModel.UserEmail}:{code}"));
				if (pageModel.ValidationErrors.Count == 0)
				{
					SignupMailModel signupMail = new SignupMailModel()
					{
						Code = code,
						User = formModel.UserName,
						Slug = slug,
						Scheme = Request.Scheme,
						Host = Request.Host.ToString()
                    };
					MailMessage mailMessage = new()
					{
						Subject = signupMail.GetSubject(),
						IsBodyHtml = true,
						Body = signupMail.GetBody()
					};
					_logger.LogInformation(mailMessage.Body);
					mailMessage.To.Add(formModel.UserEmail);
					try
					{
						_emailService.Send(mailMessage);
						String salt = RandomStringService.GenerateSalt(10);
						_dataAccessor.UserDao.Signup(new()
						{
							Name = formModel.UserName,
							Email = formModel.UserEmail,
							EmailConfirmCode = code,
							Birthdate = formModel.UserBirthdate,
							AvatarUrl = formModel.SavedAvatarFilename,
							Salt = salt,
							Derivedkey = _kdfService.DerivedKey(salt, formModel.Password)
						});
					}
					catch (Exception ex)
					{
						pageModel.ValidationErrors["email"] = "�� ������� ��������� E-mail";
						_logger.LogInformation(ex.Message);
					}


				}
			}
			//_logger.LogInformation(Directory.GetCurrentDirectory());
			return View(pageModel);
		}

		public ViewResult Admin()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}


		private Dictionary<string, string> _ValidateSingupModel(SingupFormModel? model)
		{
			Dictionary<string, string> result = new();
			if (model == null)
			{
				result["model"] = "Model is Null";
			}
			else
			{
				if (String.IsNullOrEmpty(model.UserName))
				{
					result[nameof(model.UserName)] = "User Name should not be empty";
				}
				if (String.IsNullOrEmpty(model.UserEmail))
				{
					result[nameof(model.UserEmail)] = "User Email should not be empty";
				}
				if (model.UserBirthdate == default(DateTime))
				{
					result[nameof(model.UserBirthdate)] = "User Birthdate should not be empty";
				}
				if (model.UserAvatar != null)
				{
					string ext = Path.GetExtension(model.UserAvatar.FileName);
					List<string> imageExtensions = new List<string>() { ".png", ".jpg", ".jpeg", ".svg", ".bmp", ".gif", ".webp" };
					if (!imageExtensions.Contains(ext))
						result[nameof(model.UserAvatar)] = "User Avatar must be an image type (.png, .jpg, .jpeg, .svg, .bmp, .gif, .webp)";
					// ��������� ���� � wwwroot/img/avatars � ������ �������
					// (���������� ����� �� ������������� ���������)
					String path = Directory.GetCurrentDirectory() + "/wwwroot/img/avatars/";
					_logger.LogInformation(path);
					String fileName;
					String pathName;
					do
					{
						fileName = RandomStringService.GenerateFilename(10) + ext;
						pathName = path + fileName;
					}
					while (System.IO.File.Exists(pathName));
					_logger.LogInformation(pathName);

					using var steam = System.IO.File.OpenWrite(pathName);
					model.UserAvatar.CopyTo(steam);

					model.SavedAvatarFilename = fileName;
				}
				if (!model.Agreement)
				{
					result[nameof(model.Agreement)] = "User Agreement must be checked";
				}
				if (String.IsNullOrEmpty(model.Password))
				{
					result[nameof(model.Password)] = "User Password should not be empty";
				}
				if (!String.IsNullOrEmpty(model.Password))
				{
					Regex regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d).+$");
					if (!regex.IsMatch(model.Password))
						result[nameof(model.Password)] = "User Password should contain 1 symbol and 1 digit";
				}
				if (String.IsNullOrEmpty(model.UserRepeat))
				{
					result[nameof(model.UserRepeat)] = "User Repeat Password should not be empty";
				}
				if (!String.IsNullOrEmpty(model.Password))
				{
					if (!(model.UserRepeat == model.Password))
						result[nameof(model.UserRepeat)] = "User Passwords not the same";
				}
			}
			return result;
		}
	}
}
