/*
 * COMP1551 Application Development Coursework
 * Desktop Information System for an Education Centre
 *
 * The coursework asks for one C# source file, so the forms are built in code.
 * This keeps the whole WinForms application in one file for submission.
 *
 * The program can:
 * - Add Teacher, Admin and Student records.
 * - View all records or filter them by role.
 * - Search, edit and delete records.
 * - Check input before a record is saved.
 *
 * OOP used in this program:
 * - Person is the base class.
 * - Teacher, Admin and Student inherit from Person.
 * - Private fields protect the data.
 * - Overridden methods show the correct details for each role.
 * - List<Person> stores every record.
 *
 * Records are stored in memory and are cleared when the program closes.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

namespace COMP1551Coursework
{
    // These are the three roles required by the coursework.
    public enum Role
    {
        Teacher = 1,
        Admin = 2,
        Student = 3
    }

    // Admin staff can be full-time or part-time.
    public enum EmploymentType
    {
        FullTime = 1,
        PartTime = 2
    }

    /// <summary>
    /// Keeps all validation rules in one place.
    /// </summary>
    public static class ValidationRules
    {
        public const int TeacherSubjectCount = 2;
        public const int StudentSubjectCount = 3;
        public const decimal MaximumSalary = 1000000000000m;
        public const decimal MaximumWeeklyHours = 168m;

        public static int ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id", "ID must be a positive number.");
            }

            return id;
        }

        public static Role ValidateRole(Role role)
        {
            if (!Enum.IsDefined(typeof(Role), role))
            {
                throw new ArgumentOutOfRangeException("role", "Select a valid role.");
            }

            return role;
        }

        public static string ValidateName(string name)
        {
            string value = RequireText(name, "Name");

            if (value.Length > 100)
            {
                throw new ArgumentException("Name must not exceed 100 characters.");
            }

            return value;
        }

        public static string ValidateTelephone(string telephone)
        {
            string value = RequireText(telephone, "Telephone");

            if (value.Length > 30)
            {
                throw new ArgumentException("Telephone must not exceed 30 characters.");
            }

            bool hasDigit = false;

            foreach (char character in value)
            {
                if (char.IsDigit(character))
                {
                    hasDigit = true;
                    continue;
                }

                bool allowed = char.IsWhiteSpace(character) || "+-().".IndexOf(character) >= 0;

                if (!allowed)
                {
                    throw new ArgumentException(
                        "Telephone may contain only digits, spaces, +, -, (, ) and . characters.");
                }
            }

            if (!hasDigit)
            {
                throw new ArgumentException("Telephone must contain at least one digit.");
            }

            return value;
        }

        public static string ValidateEmail(string email)
        {
            string value = RequireText(email, "Email");

            if (value.Length > 254)
            {
                throw new ArgumentException("Email must not exceed 254 characters.");
            }

            try
            {
                MailAddress address = new MailAddress(value);

                if (!string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Enter one email address without a display name.");
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException("Enter a valid email, for example name@example.com.");
            }

            return value.ToLowerInvariant();
        }

        public static decimal ValidateSalary(decimal salary)
        {
            if (salary < 0m || salary > MaximumSalary)
            {
                throw new ArgumentOutOfRangeException(
                    "salary",
                    "Salary must be between 0 and " +
                    MaximumSalary.ToString("N2", CultureInfo.CurrentCulture) + ".");
            }

            return salary;
        }

        public static EmploymentType ValidateEmploymentType(EmploymentType employmentType)
        {
            if (!Enum.IsDefined(typeof(EmploymentType), employmentType))
            {
                throw new ArgumentOutOfRangeException(
                    "employmentType",
                    "Select Full-time or Part-time employment.");
            }

            return employmentType;
        }

        public static decimal ValidateWorkingHours(decimal workingHours)
        {
            if (workingHours < 0m || workingHours > MaximumWeeklyHours)
            {
                throw new ArgumentOutOfRangeException(
                    "workingHours",
                    "Weekly working hours must be between 0 and 168.");
            }

            return workingHours;
        }

        public static string[] ValidateSubjects(
            IEnumerable<string> subjects,
            int expectedCount,
            string ownerName)
        {
            if (subjects == null)
            {
                throw new ArgumentNullException("subjects", ownerName + " subjects are required.");
            }

            string[] values = subjects.ToArray();

            if (values.Length != expectedCount)
            {
                throw new ArgumentException(
                    ownerName + " must have exactly " + expectedCount + " subjects.");
            }

            for (int index = 0; index < values.Length; index++)
            {
                values[index] = RequireText(values[index], "Subject " + (index + 1));

                if (values[index].Length > 100)
                {
                    throw new ArgumentException("A subject name must not exceed 100 characters.");
                }
            }

            if (values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != expectedCount)
            {
                throw new ArgumentException("Subject names must be different for the same person.");
            }

            return values;
        }

        // Phone numbers are compared without spaces or punctuation.
        public static string NormaliseTelephone(string telephone)
        {
            if (telephone == null)
            {
                return string.Empty;
            }

            return new string(telephone.Where(char.IsDigit).ToArray());
        }

        private static string RequireText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(fieldName + " is required.");
            }

            return value.Trim();
        }
    }

    /// <summary>
    /// Base class for every person stored by the system.
    /// </summary>
    public abstract class Person
    {
        private readonly int _id;
        private string _name;
        private string _telephone;
        private string _email;

        protected Person(int id, string name, string telephone, string email)
        {
            _id = ValidationRules.ValidateId(id);
            _name = ValidationRules.ValidateName(name);
            _telephone = ValidationRules.ValidateTelephone(telephone);
            _email = ValidationRules.ValidateEmail(email);
        }

        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Telephone
        {
            get { return _telephone; }
        }

        public string Email
        {
            get { return _email; }
        }

        public abstract Role Role { get; }

        // All new values are checked before the object is changed.
        public void UpdateCommonDetails(string name, string telephone, string email)
        {
            string validName = ValidationRules.ValidateName(name);
            string validTelephone = ValidationRules.ValidateTelephone(telephone);
            string validEmail = ValidationRules.ValidateEmail(email);

            _name = validName;
            _telephone = validTelephone;
            _email = validEmail;
        }

        public abstract string GetRoleSummary();

        public abstract string GetDetails();

        protected string GetCommonDetails()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("ID: " + Id);
            text.AppendLine("Role: " + Role);
            text.AppendLine("Name: " + Name);
            text.AppendLine("Telephone: " + Telephone);
            text.AppendLine("Email: " + Email);
            return text.ToString();
        }

        public override string ToString()
        {
            return GetDetails();
        }
    }

    /// <summary>
    /// Teacher stores salary and exactly two subjects.
    /// </summary>
    public sealed class Teacher : Person
    {
        private decimal _salary;
        private string[] _subjects;

        public Teacher(
            int id,
            string name,
            string telephone,
            string email,
            decimal salary,
            IEnumerable<string> subjects)
            : base(id, name, telephone, email)
        {
            _salary = ValidationRules.ValidateSalary(salary);
            _subjects = ValidationRules.ValidateSubjects(
                subjects,
                ValidationRules.TeacherSubjectCount,
                "Teacher");
        }

        public override Role Role
        {
            get { return global::COMP1551Coursework.Role.Teacher; }
        }

        public decimal Salary
        {
            get { return _salary; }
        }

        public string[] Subjects
        {
            get { return (string[])_subjects.Clone(); }
        }

        public void UpdateTeacherDetails(decimal salary, IEnumerable<string> subjects)
        {
            decimal validSalary = ValidationRules.ValidateSalary(salary);
            string[] validSubjects = ValidationRules.ValidateSubjects(
                subjects,
                ValidationRules.TeacherSubjectCount,
                "Teacher");

            _salary = validSalary;
            _subjects = validSubjects;
        }

        public override string GetRoleSummary()
        {
            return "Salary: " + Salary.ToString("N2", CultureInfo.CurrentCulture) +
                " | Subjects: " + string.Join(", ", Subjects);
        }

        public override string GetDetails()
        {
            return GetCommonDetails() +
                "Salary: " + Salary.ToString("N2", CultureInfo.CurrentCulture) +
                Environment.NewLine +
                "Subjects: " + string.Join(", ", Subjects);
        }
    }

    /// <summary>
    /// Admin stores salary, employment type and weekly hours.
    /// </summary>
    public sealed class Admin : Person
    {
        private decimal _salary;
        private EmploymentType _employmentType;
        private decimal _workingHours;

        public Admin(
            int id,
            string name,
            string telephone,
            string email,
            decimal salary,
            EmploymentType employmentType,
            decimal workingHours)
            : base(id, name, telephone, email)
        {
            _salary = ValidationRules.ValidateSalary(salary);
            _employmentType = ValidationRules.ValidateEmploymentType(employmentType);
            _workingHours = ValidationRules.ValidateWorkingHours(workingHours);
        }

        public override Role Role
        {
            get { return global::COMP1551Coursework.Role.Admin; }
        }

        public decimal Salary
        {
            get { return _salary; }
        }

        public EmploymentType EmploymentType
        {
            get { return _employmentType; }
        }

        public decimal WorkingHours
        {
            get { return _workingHours; }
        }

        public void UpdateAdminDetails(
            decimal salary,
            EmploymentType employmentType,
            decimal workingHours)
        {
            decimal validSalary = ValidationRules.ValidateSalary(salary);
            EmploymentType validType = ValidationRules.ValidateEmploymentType(employmentType);
            decimal validHours = ValidationRules.ValidateWorkingHours(workingHours);

            _salary = validSalary;
            _employmentType = validType;
            _workingHours = validHours;
        }

        public override string GetRoleSummary()
        {
            return "Salary: " + Salary.ToString("N2", CultureInfo.CurrentCulture) +
                " | " + FormatEmploymentType(EmploymentType) +
                " | " + WorkingHours.ToString("0.##", CultureInfo.CurrentCulture) + " hours/week";
        }

        public override string GetDetails()
        {
            return GetCommonDetails() +
                "Salary: " + Salary.ToString("N2", CultureInfo.CurrentCulture) +
                Environment.NewLine +
                "Employment: " + FormatEmploymentType(EmploymentType) +
                Environment.NewLine +
                "Weekly working hours: " +
                WorkingHours.ToString("0.##", CultureInfo.CurrentCulture);
        }

        public static string FormatEmploymentType(EmploymentType employmentType)
        {
            return employmentType == EmploymentType.FullTime ? "Full-time" : "Part-time";
        }
    }

    /// <summary>
    /// Student stores exactly three subjects.
    /// </summary>
    public sealed class Student : Person
    {
        private string[] _subjects;

        public Student(
            int id,
            string name,
            string telephone,
            string email,
            IEnumerable<string> subjects)
            : base(id, name, telephone, email)
        {
            _subjects = ValidationRules.ValidateSubjects(
                subjects,
                ValidationRules.StudentSubjectCount,
                "Student");
        }

        public override Role Role
        {
            get { return global::COMP1551Coursework.Role.Student; }
        }

        public string[] Subjects
        {
            get { return (string[])_subjects.Clone(); }
        }

        public void UpdateStudentDetails(IEnumerable<string> subjects)
        {
            _subjects = ValidationRules.ValidateSubjects(
                subjects,
                ValidationRules.StudentSubjectCount,
                "Student");
        }

        public override string GetRoleSummary()
        {
            return "Subjects: " + string.Join(", ", Subjects);
        }

        public override string GetDetails()
        {
            return GetCommonDetails() + "Subjects: " + string.Join(", ", Subjects);
        }
    }

    /// <summary>
    /// Carries validated information from the editor form to the repository.
    /// </summary>
    public sealed class PersonInput
    {
        private readonly string[] _subjects;

        public PersonInput(
            Role role,
            string name,
            string telephone,
            string email,
            decimal salary,
            EmploymentType employmentType,
            decimal workingHours,
            IEnumerable<string> subjects)
        {
            Role = ValidationRules.ValidateRole(role);
            Name = ValidationRules.ValidateName(name);
            Telephone = ValidationRules.ValidateTelephone(telephone);
            Email = ValidationRules.ValidateEmail(email);

            if (Role == global::COMP1551Coursework.Role.Teacher)
            {
                Salary = ValidationRules.ValidateSalary(salary);
                EmploymentType = global::COMP1551Coursework.EmploymentType.FullTime;
                WorkingHours = 0m;
                _subjects = ValidationRules.ValidateSubjects(
                    subjects,
                    ValidationRules.TeacherSubjectCount,
                    "Teacher");
            }
            else if (Role == global::COMP1551Coursework.Role.Admin)
            {
                Salary = ValidationRules.ValidateSalary(salary);
                EmploymentType = ValidationRules.ValidateEmploymentType(employmentType);
                WorkingHours = ValidationRules.ValidateWorkingHours(workingHours);
                _subjects = new string[0];
            }
            else
            {
                Salary = 0m;
                EmploymentType = global::COMP1551Coursework.EmploymentType.FullTime;
                WorkingHours = 0m;
                _subjects = ValidationRules.ValidateSubjects(
                    subjects,
                    ValidationRules.StudentSubjectCount,
                    "Student");
            }
        }

        public Role Role { get; private set; }

        public string Name { get; private set; }

        public string Telephone { get; private set; }

        public string Email { get; private set; }

        public decimal Salary { get; private set; }

        public EmploymentType EmploymentType { get; private set; }

        public decimal WorkingHours { get; private set; }

        public string[] Subjects
        {
            get { return (string[])_subjects.Clone(); }
        }
    }

    /// <summary>
    /// Stores all people and performs the main data operations.
    /// </summary>
    public sealed class EducationCentreRepository
    {
        private readonly List<Person> _people;
        private int _nextId;

        public EducationCentreRepository()
        {
            _people = new List<Person>();
            _nextId = 1;
        }

        public int Count
        {
            get { return _people.Count; }
        }

        // A new ID is created only after the input has been validated.
        public Person Add(PersonInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            int id = _nextId;
            Person person = CreatePerson(id, input);
            _people.Add(person);
            _nextId = checked(_nextId + 1);
            return person;
        }

        // The role is fixed after a record is created.
        public Person Update(int id, PersonInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            Person existing = FindById(id);

            if (existing == null)
            {
                throw new InvalidOperationException("The selected person no longer exists.");
            }

            if (existing.Role != input.Role)
            {
                throw new InvalidOperationException("A person's role cannot be changed during editing.");
            }

            // Create a candidate first. This checks every value before editing the stored object.
            Person candidate = CreatePerson(id, input);
            existing.UpdateCommonDetails(candidate.Name, candidate.Telephone, candidate.Email);

            Teacher existingTeacher = existing as Teacher;
            Teacher candidateTeacher = candidate as Teacher;

            if (existingTeacher != null && candidateTeacher != null)
            {
                existingTeacher.UpdateTeacherDetails(
                    candidateTeacher.Salary,
                    candidateTeacher.Subjects);
                return existingTeacher;
            }

            Admin existingAdmin = existing as Admin;
            Admin candidateAdmin = candidate as Admin;

            if (existingAdmin != null && candidateAdmin != null)
            {
                existingAdmin.UpdateAdminDetails(
                    candidateAdmin.Salary,
                    candidateAdmin.EmploymentType,
                    candidateAdmin.WorkingHours);
                return existingAdmin;
            }

            Student existingStudent = existing as Student;
            Student candidateStudent = candidate as Student;

            if (existingStudent != null && candidateStudent != null)
            {
                existingStudent.UpdateStudentDetails(candidateStudent.Subjects);
                return existingStudent;
            }

            throw new InvalidOperationException("The record type could not be updated.");
        }

        public bool Delete(int id)
        {
            int index = _people.FindIndex(person => person.Id == id);

            if (index < 0)
            {
                return false;
            }

            _people.RemoveAt(index);
            return true;
        }

        public Person FindById(int id)
        {
            return _people.FirstOrDefault(person => person.Id == id);
        }

        // The same method supports View All, View by Role and Search.
        public IList<Person> Query(Role? role, string searchText)
        {
            IEnumerable<Person> query = _people;

            if (role.HasValue)
            {
                query = query.Where(person => person.Role == role.Value);
            }

            string term = string.IsNullOrWhiteSpace(searchText)
                ? string.Empty
                : searchText.Trim();

            if (term.Length > 0)
            {
                query = query.Where(person => MatchesSearch(person, term));
            }

            return query
                .OrderBy(person => person.Id)
                .ToList()
                .AsReadOnly();
        }

        public int CountByRole(Role role)
        {
            return _people.Count(person => person.Role == role);
        }

        // A duplicate is a matching email or a matching normalised phone number.
        public IList<Person> FindPossibleDuplicates(PersonInput input, int excludedId)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            string telephone = ValidationRules.NormaliseTelephone(input.Telephone);

            return _people
                .Where(person => person.Id != excludedId)
                .Where(person =>
                    string.Equals(person.Email, input.Email, StringComparison.OrdinalIgnoreCase) ||
                    ValidationRules.NormaliseTelephone(person.Telephone) == telephone)
                .OrderBy(person => person.Id)
                .ToList()
                .AsReadOnly();
        }

        private static bool MatchesSearch(Person person, string term)
        {
            return ContainsIgnoreCase(person.Id.ToString(CultureInfo.InvariantCulture), term) ||
                ContainsIgnoreCase(person.Role.ToString(), term) ||
                ContainsIgnoreCase(person.Name, term) ||
                ContainsIgnoreCase(person.Telephone, term) ||
                ContainsIgnoreCase(person.Email, term) ||
                ContainsIgnoreCase(person.GetRoleSummary(), term);
        }

        private static bool ContainsIgnoreCase(string value, string term)
        {
            return value != null &&
                value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Person CreatePerson(int id, PersonInput input)
        {
            switch (input.Role)
            {
                case Role.Teacher:
                    return new Teacher(
                        id,
                        input.Name,
                        input.Telephone,
                        input.Email,
                        input.Salary,
                        input.Subjects);

                case Role.Admin:
                    return new Admin(
                        id,
                        input.Name,
                        input.Telephone,
                        input.Email,
                        input.Salary,
                        input.EmploymentType,
                        input.WorkingHours);

                case Role.Student:
                    return new Student(
                        id,
                        input.Name,
                        input.Telephone,
                        input.Email,
                        input.Subjects);

                default:
                    throw new InvalidOperationException("Unsupported role.");
            }
        }
    }

    /// <summary>
    /// Simple row used by the read-only DataGridView.
    /// </summary>
    public sealed class PersonGridRow
    {
        public PersonGridRow(Person person)
        {
            Id = person.Id;
            Role = person.Role.ToString();
            Name = person.Name;
            Telephone = person.Telephone;
            Email = person.Email;
            Details = person.GetRoleSummary();
        }

        public int Id { get; private set; }

        public string Role { get; private set; }

        public string Name { get; private set; }

        public string Telephone { get; private set; }

        public string Email { get; private set; }

        public string Details { get; private set; }
    }

    // Null means that all roles should be displayed.
    internal sealed class RoleFilterItem
    {
        public RoleFilterItem(string text, Role? role)
        {
            Text = text;
            Role = role;
        }

        public string Text { get; private set; }

        public Role? Role { get; private set; }

        public override string ToString()
        {
            return Text;
        }
    }

    /// <summary>
    /// Main window for viewing and managing records.
    /// </summary>
    public sealed class MainForm : Form
    {
        private readonly EducationCentreRepository _repository;

        private DataGridView _grid;
        private TextBox _searchTextBox;
        private ComboBox _roleFilterComboBox;
        private Label _totalLabel;
        private Label _teacherLabel;
        private Label _adminLabel;
        private Label _studentLabel;
        private Button _detailsButton;
        private Button _editButton;
        private Button _deleteButton;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _selectionLabel;

        public MainForm(EducationCentreRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
            InitialiseWindow();
            BuildInterface();
            WireEvents();
            RefreshGrid(null);
        }

        private void InitialiseWindow()
        {
            Text = "Education Centre Information System";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(980, 620);
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        // The form uses layout panels so it resizes more reliably.
        private void BuildInterface()
        {
            MenuStrip menu = BuildMenu();
            StatusStrip status = BuildStatusStrip();

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(16, 14, 16, 12);
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 75f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildSummaryPanel(), 0, 1);
            root.Controls.Add(BuildToolbar(), 0, 2);
            root.Controls.Add(BuildGrid(), 0, 3);

            Controls.Add(root);
            Controls.Add(status);
            Controls.Add(menu);
            MainMenuStrip = menu;
        }

        private MenuStrip BuildMenu()
        {
            MenuStrip menu = new MenuStrip();

            ToolStripMenuItem recordsMenu = new ToolStripMenuItem("&Records");
            recordsMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&Add person...",
                null,
                delegate { AddPerson(); },
                Keys.Control | Keys.N));
            recordsMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&View details",
                null,
                delegate { ViewSelectedPerson(); },
                Keys.Control | Keys.D));
            recordsMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&Edit person...",
                null,
                delegate { EditSelectedPerson(); },
                Keys.Control | Keys.E));
            recordsMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&Delete person...",
                null,
                delegate { DeleteSelectedPerson(); },
                Keys.Delete));
            recordsMenu.DropDownItems.Add(new ToolStripSeparator());
            recordsMenu.DropDownItems.Add(new ToolStripMenuItem(
                "E&xit",
                null,
                delegate { Close(); }));

            ToolStripMenuItem viewMenu = new ToolStripMenuItem("&View");
            viewMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&All people",
                null,
                delegate { ApplyRoleFilter(null); },
                Keys.Control | Keys.D0));
            viewMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&Teachers",
                null,
                delegate { ApplyRoleFilter(Role.Teacher); },
                Keys.Control | Keys.D1));
            viewMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&Admins",
                null,
                delegate { ApplyRoleFilter(Role.Admin); },
                Keys.Control | Keys.D2));
            viewMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&Students",
                null,
                delegate { ApplyRoleFilter(Role.Student); },
                Keys.Control | Keys.D3));
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&Refresh",
                null,
                delegate { RefreshGrid(GetSelectedId()); },
                Keys.F5));

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem(
                "&About",
                null,
                delegate { ShowAbout(); }));

            menu.Items.Add(recordsMenu);
            menu.Items.Add(viewMenu);
            menu.Items.Add(helpMenu);
            return menu;
        }

        private StatusStrip BuildStatusStrip()
        {
            StatusStrip status = new StatusStrip();

            _statusLabel = new ToolStripStatusLabel();
            _statusLabel.Spring = true;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.Text = "Ready";

            _selectionLabel = new ToolStripStatusLabel();
            _selectionLabel.Text = "No record selected";

            ToolStripStatusLabel storageLabel = new ToolStripStatusLabel();
            storageLabel.Text = "In-memory storage";

            status.Items.Add(_statusLabel);
            status.Items.Add(_selectionLabel);
            status.Items.Add(new ToolStripStatusLabel(" | "));
            status.Items.Add(storageLabel);
            return status;
        }

        private Control BuildHeader()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(34, 79, 142);
            panel.Margin = new Padding(0, 0, 0, 10);

            Label title = new Label();
            title.AutoSize = true;
            title.Location = new Point(20, 10);
            title.Text = "Education Centre Information System";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold);

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.Location = new Point(22, 43);
            subtitle.Text = "Manage Teacher, Admin and Student records";
            subtitle.ForeColor = Color.FromArgb(225, 235, 248);

            panel.Controls.Add(title);
            panel.Controls.Add(subtitle);
            return panel;
        }

        private Control BuildSummaryPanel()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 4;
            panel.RowCount = 1;
            panel.Margin = new Padding(0);

            for (int index = 0; index < 4; index++)
            {
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }

            panel.Controls.Add(CreateSummaryBox("TOTAL", out _totalLabel), 0, 0);
            panel.Controls.Add(CreateSummaryBox("TEACHERS", out _teacherLabel), 1, 0);
            panel.Controls.Add(CreateSummaryBox("ADMINS", out _adminLabel), 2, 0);
            panel.Controls.Add(CreateSummaryBox("STUDENTS", out _studentLabel), 3, 0);
            return panel;
        }

        private static Control CreateSummaryBox(string title, out Label valueLabel)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Margin = new Padding(0, 0, 10, 10);

            Label titleLabel = new Label();
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(14, 10);
            titleLabel.Text = title;
            titleLabel.ForeColor = Color.DimGray;
            titleLabel.Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold);

            valueLabel = new Label();
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(12, 29);
            valueLabel.Text = "0";
            valueLabel.Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold);

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(valueLabel);
            return panel;
        }

        private Control BuildToolbar()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(10, 9, 10, 8);
            panel.Margin = new Padding(0, 0, 0, 10);
            panel.ColumnCount = 6;
            panel.RowCount = 1;
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420f));

            Label searchLabel = CreateToolbarLabel("Search");
            _searchTextBox = new TextBox();
            _searchTextBox.Dock = DockStyle.Fill;
            _searchTextBox.Margin = new Padding(0, 4, 10, 4);
            _searchTextBox.MaxLength = 150;

            Label roleLabel = CreateToolbarLabel("Role");
            _roleFilterComboBox = new ComboBox();
            _roleFilterComboBox.Dock = DockStyle.Fill;
            _roleFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _roleFilterComboBox.Margin = new Padding(0, 3, 10, 4);
            _roleFilterComboBox.Items.Add(new RoleFilterItem("All roles", null));
            _roleFilterComboBox.Items.Add(new RoleFilterItem("Teachers", Role.Teacher));
            _roleFilterComboBox.Items.Add(new RoleFilterItem("Admins", Role.Admin));
            _roleFilterComboBox.Items.Add(new RoleFilterItem("Students", Role.Student));
            _roleFilterComboBox.SelectedIndex = 0;

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.WrapContents = false;
            actions.Margin = new Padding(0);

            Button addButton = CreateButton("Add", 74);
            _detailsButton = CreateButton("Details", 74);
            _editButton = CreateButton("Edit", 70);
            _deleteButton = CreateButton("Delete", 74);
            Button refreshButton = CreateButton("Refresh", 76);

            addButton.Click += delegate { AddPerson(); };
            _detailsButton.Click += delegate { ViewSelectedPerson(); };
            _editButton.Click += delegate { EditSelectedPerson(); };
            _deleteButton.Click += delegate { DeleteSelectedPerson(); };
            refreshButton.Click += delegate { RefreshGrid(GetSelectedId()); };

            actions.Controls.Add(refreshButton);
            actions.Controls.Add(_deleteButton);
            actions.Controls.Add(_editButton);
            actions.Controls.Add(_detailsButton);
            actions.Controls.Add(addButton);

            panel.Controls.Add(searchLabel, 0, 0);
            panel.Controls.Add(_searchTextBox, 1, 0);
            panel.Controls.Add(roleLabel, 2, 0);
            panel.Controls.Add(_roleFilterComboBox, 3, 0);
            panel.Controls.Add(new Panel(), 4, 0);
            panel.Controls.Add(actions, 5, 0);
            return panel;
        }

        private static Label CreateToolbarLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private static Button CreateButton(string text, int width)
        {
            Button button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = 31;
            button.Margin = new Padding(5, 0, 0, 0);
            button.FlatStyle = FlatStyle.System;
            return button;
        }

        private Control BuildGrid()
        {
            _grid = new DataGridView();
            _grid.Dock = DockStyle.Fill;
            _grid.BackgroundColor = Color.White;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            _grid.AutoGenerateColumns = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.ReadOnly = true;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.RowHeadersVisible = false;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _grid.RowTemplate.Height = 30;

            _grid.Columns.Add(CreateColumn("Id", "ID", 55, false));
            _grid.Columns.Add(CreateColumn("Role", "Role", 85, false));
            _grid.Columns.Add(CreateColumn("Name", "Name", 150, true));
            _grid.Columns.Add(CreateColumn("Telephone", "Telephone", 130, false));
            _grid.Columns.Add(CreateColumn("Email", "Email", 190, true));
            _grid.Columns.Add(CreateColumn("Details", "Role-specific details", 280, true));
            return _grid;
        }

        private static DataGridViewTextBoxColumn CreateColumn(
            string property,
            string heading,
            int width,
            bool fill)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = property;
            column.HeaderText = heading;
            column.Width = width;
            column.MinimumWidth = Math.Min(width, 70);
            column.ReadOnly = true;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.AutoSizeMode = fill
                ? DataGridViewAutoSizeColumnMode.Fill
                : DataGridViewAutoSizeColumnMode.None;
            return column;
        }

        private void WireEvents()
        {
            _searchTextBox.TextChanged += delegate { RefreshGrid(null); };
            _roleFilterComboBox.SelectedIndexChanged += delegate { RefreshGrid(null); };
            _grid.SelectionChanged += delegate { UpdateSelectionState(); };

            _grid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs eventArgs)
            {
                if (eventArgs.RowIndex >= 0)
                {
                    EditSelectedPerson();
                }
            };

            _grid.KeyDown += delegate(object sender, KeyEventArgs eventArgs)
            {
                if (eventArgs.KeyCode == Keys.Delete)
                {
                    DeleteSelectedPerson();
                    eventArgs.Handled = true;
                    eventArgs.SuppressKeyPress = true;
                }
                else if (eventArgs.KeyCode == Keys.Enter)
                {
                    ViewSelectedPerson();
                    eventArgs.Handled = true;
                    eventArgs.SuppressKeyPress = true;
                }
            };
        }

        // Rebuilds the table and updates the four record counters.
        private void RefreshGrid(int? selectId)
        {
            RoleFilterItem filter = _roleFilterComboBox.SelectedItem as RoleFilterItem;
            Role? role = filter == null ? null : filter.Role;
            IList<Person> people = _repository.Query(role, _searchTextBox.Text);

            List<PersonGridRow> rows = people
                .Select(person => new PersonGridRow(person))
                .ToList();

            _grid.DataSource = null;
            _grid.DataSource = rows;

            _totalLabel.Text = _repository.Count.ToString(CultureInfo.InvariantCulture);
            _teacherLabel.Text = _repository
                .CountByRole(Role.Teacher)
                .ToString(CultureInfo.InvariantCulture);
            _adminLabel.Text = _repository
                .CountByRole(Role.Admin)
                .ToString(CultureInfo.InvariantCulture);
            _studentLabel.Text = _repository
                .CountByRole(Role.Student)
                .ToString(CultureInfo.InvariantCulture);

            _statusLabel.Text = "Showing " + rows.Count + " of " + _repository.Count + " records";

            if (selectId.HasValue)
            {
                SelectRow(selectId.Value);
            }
            else
            {
                _grid.ClearSelection();
            }

            UpdateSelectionState();
        }

        private void SelectRow(int id)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                PersonGridRow item = row.DataBoundItem as PersonGridRow;

                if (item != null && item.Id == id)
                {
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[0];
                    return;
                }
            }

            _grid.ClearSelection();
        }

        private int? GetSelectedId()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                return null;
            }

            PersonGridRow row = _grid.SelectedRows[0].DataBoundItem as PersonGridRow;
            return row == null ? (int?)null : row.Id;
        }

        private Person GetSelectedPerson()
        {
            int? id = GetSelectedId();
            return id.HasValue ? _repository.FindById(id.Value) : null;
        }

        private void UpdateSelectionState()
        {
            int? id = GetSelectedId();
            bool hasSelection = id.HasValue;

            _detailsButton.Enabled = hasSelection;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _selectionLabel.Text = hasSelection
                ? "Selected ID: " + id.Value
                : "No record selected";
        }

        private void AddPerson()
        {
            using (PersonEditorForm editor = new PersonEditorForm(null))
            {
                if (editor.ShowDialog(this) != DialogResult.OK)
                {
                    SetStatus("Add operation cancelled.");
                    return;
                }

                if (!ConfirmPossibleDuplicate(editor.Result, 0))
                {
                    SetStatus("Add operation cancelled after duplicate warning.");
                    return;
                }

                try
                {
                    Person added = _repository.Add(editor.Result);
                    ClearFilters();
                    RefreshGrid(added.Id);
                    SetStatus("Person ID " + added.Id + " was added successfully.");
                }
                catch (Exception exception)
                {
                    ShowOperationError("The person could not be added.", exception);
                }
            }
        }

        private void ViewSelectedPerson()
        {
            Person person = GetSelectedPerson();

            if (person == null)
            {
                ShowSelectionRequired("view");
                return;
            }

            MessageBox.Show(
                this,
                person.GetDetails(),
                "Person details - ID " + person.Id,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void EditSelectedPerson()
        {
            Person person = GetSelectedPerson();

            if (person == null)
            {
                ShowSelectionRequired("edit");
                return;
            }

            using (PersonEditorForm editor = new PersonEditorForm(person))
            {
                if (editor.ShowDialog(this) != DialogResult.OK)
                {
                    SetStatus("Edit operation cancelled. No data was changed.");
                    return;
                }

                if (!ConfirmPossibleDuplicate(editor.Result, person.Id))
                {
                    SetStatus("Edit operation cancelled after duplicate warning.");
                    return;
                }

                try
                {
                    Person updated = _repository.Update(person.Id, editor.Result);
                    RefreshGrid(updated.Id);
                    SetStatus("Person ID " + updated.Id + " was updated successfully.");
                }
                catch (Exception exception)
                {
                    ShowOperationError("The person could not be updated.", exception);
                }
            }
        }

        private void DeleteSelectedPerson()
        {
            Person person = GetSelectedPerson();

            if (person == null)
            {
                ShowSelectionRequired("delete");
                return;
            }

            string message =
                "Delete this record permanently?" + Environment.NewLine +
                Environment.NewLine +
                "ID: " + person.Id + Environment.NewLine +
                "Role: " + person.Role + Environment.NewLine +
                "Name: " + person.Name + Environment.NewLine +
                "Email: " + person.Email;

            DialogResult answer = MessageBox.Show(
                this,
                message,
                "Confirm deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (answer != DialogResult.Yes)
            {
                SetStatus("Delete operation cancelled. No data was changed.");
                return;
            }

            if (_repository.Delete(person.Id))
            {
                RefreshGrid(null);
                SetStatus("Person ID " + person.Id + " was deleted successfully.");
            }
            else
            {
                MessageBox.Show(
                    this,
                    "The selected person no longer exists.",
                    "Delete failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool ConfirmPossibleDuplicate(PersonInput input, int excludedId)
        {
            IList<Person> duplicates = _repository.FindPossibleDuplicates(input, excludedId);

            if (duplicates.Count == 0)
            {
                return true;
            }

            StringBuilder message = new StringBuilder();
            message.AppendLine("Possible duplicate contact details were found:");
            message.AppendLine();

            foreach (Person person in duplicates)
            {
                message.AppendLine(
                    "ID " + person.Id + " - " + person.Name + " (" + person.Role + ")");
            }

            message.AppendLine();
            message.Append("Save the record anyway?");

            return MessageBox.Show(
                this,
                message.ToString(),
                "Possible duplicate",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void ApplyRoleFilter(Role? role)
        {
            for (int index = 0; index < _roleFilterComboBox.Items.Count; index++)
            {
                RoleFilterItem item = _roleFilterComboBox.Items[index] as RoleFilterItem;

                if (item != null && item.Role == role)
                {
                    _roleFilterComboBox.SelectedIndex = index;
                    return;
                }
            }
        }

        private void ClearFilters()
        {
            _searchTextBox.Text = string.Empty;
            _roleFilterComboBox.SelectedIndex = 0;
        }

        private void ShowSelectionRequired(string operation)
        {
            MessageBox.Show(
                this,
                "Select a record before choosing " + operation + ".",
                "Selection required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SetStatus(string message)
        {
            _statusLabel.Text = message;
        }

        private void ShowOperationError(string heading, Exception exception)
        {
            MessageBox.Show(
                this,
                heading + Environment.NewLine + Environment.NewLine + exception.Message,
                "Operation failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            SetStatus(heading);
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                this,
                "Education Centre Information System" + Environment.NewLine +
                "COMP1551 coursework" + Environment.NewLine +
                Environment.NewLine +
                "Uses encapsulation, inheritance and polymorphism.",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // The user is warned because data is not saved after the program closes.
        protected override void OnFormClosing(FormClosingEventArgs eventArgs)
        {
            if (eventArgs.CloseReason == CloseReason.UserClosing && _repository.Count > 0)
            {
                DialogResult answer = MessageBox.Show(
                    this,
                    "Records are stored only for this session and will be lost after exit." +
                    Environment.NewLine + Environment.NewLine +
                    "Exit the application?",
                    "Confirm exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (answer != DialogResult.Yes)
                {
                    eventArgs.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(eventArgs);
        }
    }

    /// <summary>
    /// Form used for both adding and editing a person.
    /// </summary>
    public sealed class PersonEditorForm : Form
    {
        private readonly Person _existingPerson;
        private readonly ErrorProvider _errorProvider;

        private ComboBox _roleComboBox;
        private TextBox _nameTextBox;
        private TextBox _telephoneTextBox;
        private TextBox _emailTextBox;

        private Panel _roleDetailsHost;
        private Panel _teacherPanel;
        private NumericUpDown _teacherSalaryInput;
        private TextBox _teacherSubject1TextBox;
        private TextBox _teacherSubject2TextBox;

        private Panel _adminPanel;
        private NumericUpDown _adminSalaryInput;
        private ComboBox _employmentTypeComboBox;
        private NumericUpDown _workingHoursInput;

        private Panel _studentPanel;
        private TextBox _studentSubject1TextBox;
        private TextBox _studentSubject2TextBox;
        private TextBox _studentSubject3TextBox;

        private Label _validationLabel;

        public PersonEditorForm(Person existingPerson)
        {
            _existingPerson = existingPerson;
            _errorProvider = new ErrorProvider();
            _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            _errorProvider.ContainerControl = this;

            InitialiseWindow();
            BuildInterface();
            PopulateExistingValues();
            ShowSelectedRolePanel();
        }

        public PersonInput Result { get; private set; }

        private void InitialiseWindow()
        {
            Text = _existingPerson == null ? "Add person" : "Edit person";
            StartPosition = FormStartPosition.CenterParent;

            // The form can resize, which prevents controls being cut off by DPI scaling.
            ClientSize = new Size(760, 650);
            MinimumSize = new Size(700, 600);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            AutoScaleDimensions = new SizeF(96f, 96f);
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void BuildInterface()
        {
            // The buttons stay in a fixed row. The input area scrolls when needed.
            TableLayoutPanel page = new TableLayoutPanel();
            page.Dock = DockStyle.Fill;
            page.Padding = new Padding(16);
            page.ColumnCount = 1;
            page.RowCount = 2;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));

            Panel scrollHost = new Panel();
            scrollHost.Dock = DockStyle.Fill;
            scrollHost.AutoScroll = true;
            scrollHost.BackColor = BackColor;
            scrollHost.Margin = new Padding(0);

            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Top;
            content.AutoSize = true;
            content.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            content.ColumnCount = 1;
            content.RowCount = 4;
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 205f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 215f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));

            content.Controls.Add(BuildHeader(), 0, 0);
            content.Controls.Add(BuildCommonDetails(), 0, 1);
            content.Controls.Add(BuildRoleDetails(), 0, 2);
            content.Controls.Add(BuildValidationLabel(), 0, 3);

            scrollHost.Controls.Add(content);
            page.Controls.Add(scrollHost, 0, 0);
            page.Controls.Add(BuildButtons(), 0, 1);
            Controls.Add(page);
        }

        private Control BuildHeader()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(34, 79, 142);
            panel.Margin = new Padding(0, 0, 0, 8);

            Label title = new Label();
            title.AutoSize = true;
            title.Location = new Point(16, 8);
            title.Text = _existingPerson == null ? "Add a new person" : "Edit person";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI Semibold", 15f, FontStyle.Bold);

            Label subtitle = new Label();
            subtitle.AutoSize = true;
            subtitle.Location = new Point(18, 35);
            subtitle.ForeColor = Color.FromArgb(225, 235, 248);
            subtitle.Text = _existingPerson == null
                ? "A unique ID will be created automatically."
                : "Editing ID " + _existingPerson.Id + ". The role cannot be changed.";

            panel.Controls.Add(title);
            panel.Controls.Add(subtitle);
            return panel;
        }

        private Control BuildCommonDetails()
        {
            GroupBox group = new GroupBox();
            group.Text = "Common details";
            group.Dock = DockStyle.Fill;
            group.BackColor = Color.White;
            group.Margin = new Padding(0, 0, 0, 8);
            group.Padding = new Padding(12);

            TableLayoutPanel table = CreateInputTable(4);

            _roleComboBox = new ComboBox();
            _roleComboBox.Dock = DockStyle.Fill;
            _roleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _roleComboBox.Items.Add(Role.Teacher);
            _roleComboBox.Items.Add(Role.Admin);
            _roleComboBox.Items.Add(Role.Student);
            _roleComboBox.SelectedIndexChanged += delegate { ShowSelectedRolePanel(); };
            _roleComboBox.SelectedIndex = 0;

            _nameTextBox = CreateTextBox(100);
            _telephoneTextBox = CreateTextBox(30);
            _emailTextBox = CreateTextBox(254);

            AddInputRow(table, 0, "Role *", _roleComboBox);
            AddInputRow(table, 1, "Name *", _nameTextBox);
            AddInputRow(table, 2, "Telephone *", _telephoneTextBox);
            AddInputRow(table, 3, "Email *", _emailTextBox);

            group.Controls.Add(table);
            return group;
        }

        private Control BuildRoleDetails()
        {
            GroupBox group = new GroupBox();
            group.Text = "Role-specific details";
            group.Dock = DockStyle.Fill;
            group.BackColor = Color.White;
            group.Margin = new Padding(0, 0, 0, 8);
            group.Padding = new Padding(12);

            _roleDetailsHost = new Panel();
            _roleDetailsHost.Dock = DockStyle.Fill;

            _teacherPanel = BuildTeacherPanel();
            _adminPanel = BuildAdminPanel();
            _studentPanel = BuildStudentPanel();

            // All role panels remain attached to the same host. The selected one is
            // shown and brought to the front; the others are hidden.
            _roleDetailsHost.Controls.Add(_studentPanel);
            _roleDetailsHost.Controls.Add(_adminPanel);
            _roleDetailsHost.Controls.Add(_teacherPanel);
            group.Controls.Add(_roleDetailsHost);
            return group;
        }

        private Panel BuildTeacherPanel()
        {
            Panel panel = CreateRolePanel();
            TableLayoutPanel table = CreateInputTable(3);

            _teacherSalaryInput = CreateMoneyInput();
            _teacherSubject1TextBox = CreateTextBox(100);
            _teacherSubject2TextBox = CreateTextBox(100);

            AddInputRow(table, 0, "Salary *", _teacherSalaryInput);
            AddInputRow(table, 1, "Subject 1 *", _teacherSubject1TextBox);
            AddInputRow(table, 2, "Subject 2 *", _teacherSubject2TextBox);
            panel.Controls.Add(table);
            return panel;
        }

        private Panel BuildAdminPanel()
        {
            Panel panel = CreateRolePanel();
            TableLayoutPanel table = CreateInputTable(3);

            _adminSalaryInput = CreateMoneyInput();

            _employmentTypeComboBox = new ComboBox();
            _employmentTypeComboBox.Dock = DockStyle.Fill;
            _employmentTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _employmentTypeComboBox.DataSource = Enum.GetValues(typeof(EmploymentType));
            _employmentTypeComboBox.Format += delegate(object sender, ListControlConvertEventArgs eventArgs)
            {
                if (eventArgs.ListItem is EmploymentType)
                {
                    eventArgs.Value = Admin.FormatEmploymentType((EmploymentType)eventArgs.ListItem);
                }
            };

            _workingHoursInput = new NumericUpDown();
            _workingHoursInput.Dock = DockStyle.Fill;
            _workingHoursInput.Minimum = 0m;
            _workingHoursInput.Maximum = ValidationRules.MaximumWeeklyHours;
            _workingHoursInput.DecimalPlaces = 2;
            _workingHoursInput.Increment = 0.5m;

            AddInputRow(table, 0, "Salary *", _adminSalaryInput);
            AddInputRow(table, 1, "Employment *", _employmentTypeComboBox);
            AddInputRow(table, 2, "Hours/week *", _workingHoursInput);
            panel.Controls.Add(table);
            return panel;
        }

        private Panel BuildStudentPanel()
        {
            Panel panel = CreateRolePanel();
            TableLayoutPanel table = CreateInputTable(3);

            _studentSubject1TextBox = CreateTextBox(100);
            _studentSubject2TextBox = CreateTextBox(100);
            _studentSubject3TextBox = CreateTextBox(100);

            AddInputRow(table, 0, "Subject 1 *", _studentSubject1TextBox);
            AddInputRow(table, 1, "Subject 2 *", _studentSubject2TextBox);
            AddInputRow(table, 2, "Subject 3 *", _studentSubject3TextBox);
            panel.Controls.Add(table);
            return panel;
        }

        private Control BuildValidationLabel()
        {
            _validationLabel = new Label();
            _validationLabel.Dock = DockStyle.Fill;
            _validationLabel.ForeColor = Color.Firebrick;
            _validationLabel.TextAlign = ContentAlignment.MiddleLeft;
            return _validationLabel;
        }

        private Control BuildButtons()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.RightToLeft;
            panel.WrapContents = false;
            panel.Padding = new Padding(0, 8, 0, 0);
            panel.Margin = new Padding(0);

            Button saveButton = new Button();
            saveButton.Text = _existingPerson == null ? "Add person" : "Save changes";
            saveButton.Size = new Size(120, 34);
            saveButton.BackColor = Color.FromArgb(34, 139, 94);
            saveButton.ForeColor = Color.White;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += delegate { ValidateAndClose(); };

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(90, 34);
            cancelButton.BackColor = Color.White;
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.DialogResult = DialogResult.Cancel;

            panel.Controls.Add(saveButton);
            panel.Controls.Add(cancelButton);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
            return panel;
        }

        private static TableLayoutPanel CreateInputTable(int rows)
        {
            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 2;
            table.RowCount = rows;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            for (int index = 0; index < rows; index++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            }

            return table;
        }

        private static void AddInputRow(
            TableLayoutPanel table,
            int row,
            string labelText,
            Control input)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Margin = new Padding(0, 2, 8, 2);

            input.Margin = new Padding(0, 5, 18, 5);
            table.Controls.Add(label, 0, row);
            table.Controls.Add(input, 1, row);
        }

        private static TextBox CreateTextBox(int maximumLength)
        {
            TextBox textBox = new TextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.MaxLength = maximumLength;
            return textBox;
        }

        private static NumericUpDown CreateMoneyInput()
        {
            NumericUpDown input = new NumericUpDown();
            input.Dock = DockStyle.Fill;
            input.Minimum = 0m;
            input.Maximum = ValidationRules.MaximumSalary;
            input.DecimalPlaces = 2;
            input.Increment = 100m;
            input.ThousandsSeparator = true;
            return input;
        }

        private static Panel CreateRolePanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            return panel;
        }

        private void PopulateExistingValues()
        {
            if (_existingPerson == null)
            {
                SelectRole(Role.Teacher);
                _employmentTypeComboBox.SelectedItem = EmploymentType.FullTime;
                ShowSelectedRolePanel();
                return;
            }

            // Select the role by searching the ComboBox items instead of relying on
            // SelectedItem assignment. This is reliable for all enum values.
            SelectRole(_existingPerson.Role);
            _roleComboBox.Enabled = false;
            _nameTextBox.Text = _existingPerson.Name;
            _telephoneTextBox.Text = _existingPerson.Telephone;
            _emailTextBox.Text = _existingPerson.Email;

            Teacher teacher = _existingPerson as Teacher;

            if (teacher != null)
            {
                _teacherSalaryInput.Value = teacher.Salary;
                _teacherSubject1TextBox.Text = teacher.Subjects[0];
                _teacherSubject2TextBox.Text = teacher.Subjects[1];
            }
            else
            {
                Admin admin = _existingPerson as Admin;

                if (admin != null)
                {
                    _adminSalaryInput.Value = admin.Salary;
                    _employmentTypeComboBox.SelectedItem = admin.EmploymentType;
                    _workingHoursInput.Value = admin.WorkingHours;
                }
                else
                {
                    Student student = _existingPerson as Student;

                    if (student != null)
                    {
                        _studentSubject1TextBox.Text = student.Subjects[0];
                        _studentSubject2TextBox.Text = student.Subjects[1];
                        _studentSubject3TextBox.Text = student.Subjects[2];
                    }
                }
            }

            ShowSelectedRolePanel();
        }

        private void SelectRole(Role role)
        {
            switch (role)
            {
                case Role.Teacher:
                    _roleComboBox.SelectedIndex = 0;
                    break;

                case Role.Admin:
                    _roleComboBox.SelectedIndex = 1;
                    break;

                case Role.Student:
                    _roleComboBox.SelectedIndex = 2;
                    break;

                default:
                    _roleComboBox.SelectedIndex = 0;
                    break;
            }
        }

        private Role GetSelectedRole()
        {
            switch (_roleComboBox.SelectedIndex)
            {
                case 1:
                    return Role.Admin;

                case 2:
                    return Role.Student;

                default:
                    return Role.Teacher;
            }
        }

        private void ShowSelectedRolePanel()
        {
            if (_roleDetailsHost == null ||
                _teacherPanel == null ||
                _adminPanel == null ||
                _studentPanel == null)
            {
                return;
            }

            Role role = GetSelectedRole();

            _roleDetailsHost.SuspendLayout();

            _teacherPanel.Visible = false;
            _adminPanel.Visible = false;
            _studentPanel.Visible = false;

            if (role == Role.Teacher)
            {
                _teacherPanel.Visible = true;
                _teacherPanel.BringToFront();
            }
            else if (role == Role.Admin)
            {
                _adminPanel.Visible = true;
                _adminPanel.BringToFront();
            }
            else
            {
                _studentPanel.Visible = true;
                _studentPanel.BringToFront();
            }

            _roleDetailsHost.ResumeLayout(true);

            if (_validationLabel != null)
            {
                _validationLabel.Text = string.Empty;
            }

            _errorProvider.Clear();
        }

        // Checks the visible inputs and closes only when all values are valid.
        private void ValidateAndClose()
        {
            _errorProvider.Clear();
            _validationLabel.Text = string.Empty;

            bool valid = true;
            Role role = GetSelectedRole();
            string name = ValidateTextBox(_nameTextBox, ValidationRules.ValidateName, ref valid);
            string telephone = ValidateTextBox(
                _telephoneTextBox,
                ValidationRules.ValidateTelephone,
                ref valid);
            string email = ValidateTextBox(_emailTextBox, ValidationRules.ValidateEmail, ref valid);

            decimal salary = 0m;
            EmploymentType employmentType = EmploymentType.FullTime;
            decimal workingHours = 0m;
            string[] subjects = new string[0];

            if (role == Role.Teacher)
            {
                salary = _teacherSalaryInput.Value;
                subjects = ValidateSubjectTextBoxes(
                    new[] { _teacherSubject1TextBox, _teacherSubject2TextBox },
                    "Teacher",
                    ref valid);
            }
            else if (role == Role.Admin)
            {
                salary = _adminSalaryInput.Value;
                employmentType = _employmentTypeComboBox.SelectedItem is EmploymentType
                    ? (EmploymentType)_employmentTypeComboBox.SelectedItem
                    : EmploymentType.FullTime;
                workingHours = _workingHoursInput.Value;
            }
            else
            {
                subjects = ValidateSubjectTextBoxes(
                    new[]
                    {
                        _studentSubject1TextBox,
                        _studentSubject2TextBox,
                        _studentSubject3TextBox
                    },
                    "Student",
                    ref valid);
            }

            if (!valid)
            {
                _validationLabel.Text = "Please correct the highlighted fields before saving.";
                return;
            }

            try
            {
                Result = new PersonInput(
                    role,
                    name,
                    telephone,
                    email,
                    salary,
                    employmentType,
                    workingHours,
                    subjects);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException exception)
            {
                _validationLabel.Text = exception.Message;
            }
        }

        private string ValidateTextBox(
            TextBox textBox,
            Func<string, string> validator,
            ref bool overallResult)
        {
            try
            {
                return validator(textBox.Text);
            }
            catch (ArgumentException exception)
            {
                _errorProvider.SetError(textBox, exception.Message);
                overallResult = false;
                return textBox.Text;
            }
        }

        private string[] ValidateSubjectTextBoxes(
            TextBox[] textBoxes,
            string ownerName,
            ref bool overallResult)
        {
            string[] values = textBoxes.Select(textBox => textBox.Text).ToArray();

            try
            {
                return ValidationRules.ValidateSubjects(values, textBoxes.Length, ownerName);
            }
            catch (ArgumentException exception)
            {
                foreach (TextBox textBox in textBoxes)
                {
                    _errorProvider.SetError(textBox, exception.Message);
                }

                overallResult = false;
                return values;
            }
        }

        // ErrorProvider was created in code, so it is disposed here.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _errorProvider.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Starts the Windows Forms application.
    /// </summary>
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                EducationCentreRepository repository = new EducationCentreRepository();
                Application.Run(new MainForm(repository));
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "The application encountered an unexpected error:" +
                    Environment.NewLine + Environment.NewLine +
                    exception.Message,
                    "Application error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
