using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;															// для работы с	файлами
using Microsoft.WindowsAPICodePack.Dialogs;									// для более юзабельного диалога открытия файла/папки

using ScintillaNET;
using VPKSoft.ScintillaLexers.CreateSpecificLexer;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.Win32;

namespace test_ScintillaNET
{
	public partial class Form1 : Form
	{

		string path;														// строка "путь	+ имя файла"
		int	lastCaretPos = 0;												// хранится	символ последней позиции (подсветка	парных скобок)

		IniFile	INI	= new IniFile("myconf.ini");
		int	form_top, form_left	= 0;                                        // положение окна приложения

		CommonOpenFileDialog opn = new CommonOpenFileDialog();              // создать объект
		CommonSaveFileDialog sav = new CommonSaveFileDialog();              // создать объект
//		sav.Filters.Add(new CommonFileDialogFilter("Word Documents", "*.docx"));
//		sav.Filters.Add(new CommonFileDialogFilter("JPEG Files", "*.jpg"));




		public Form1()
		{
			InitializeComponent();
		}


		/*		private	void LoadDataFromFile(string path)
				{
					if (File.Exists(path))
					{
						FileName.Text =	Path.GetFileName(path);
						TextArea.Text =	File.ReadAllText(path);
					}
				}
		*/




		// Создание	формы приложения
		private	void Form1_Load(object sender, EventArgs e)
		{

			// настройка свойств компонента	'Scintilla'
			InitSyntaxColoring();

			// включение области Номеров строк (NUMBER MARGIN)
			InitNumberMargin();

			// включение области Bookmarks (BOOKMARK MARGIN)
			InitBookmarkMargin();

			// включение области Code_Folding (CODE	FOLDING	MARGIN)
			InitCodeFolding();


			// фильтры диалога сохранения файла
			sav.Filters.Add(new CommonFileDialogFilter("INI Files", "*.ini"));
			sav.Filters.Add(new CommonFileDialogFilter("JPEG Files", "*.jpg"));
			sav.Filters.Add(new CommonFileDialogFilter("Word Documents", "*.docx"));
			sav.Filters.Add(new CommonFileDialogFilter("All Files", "*.*"));



			//--- свойства компонента 'Scintilla'

			// убрать бордюр области редактора
			scintilla1.BorderStyle = BorderStyle.None;

			// правая верт.	линия
			scintilla1.EdgeMode	= EdgeMode.Line;									// включить	линию (другие гуано)
			scintilla1.EdgeColumn =	90;												// колонка
			scintilla1.EdgeColor = IntToColor(0x404040);							// цвет

			scintilla1.UseTabs = true;
			scintilla1.TabWidth	= 4;												// шаг табуляции

			// опускание ниже последней	строки (при	листании страницей)
			scintilla1.EndAtLastLine = false;										// true/false =	нет/да


			//scintilla1.LineEndTypesAllowed[Unicode]
			//scintilla1.convert





			//path = "C:/rom_cv-utf8.c";											// путь	+ файл
			path = "../../../rom_cv-utf8.c";										// путь	+ файл

			//-------------------------------------------------------
			//	Загрузка файла
			//-------------------------------------------------------
			if (File.Exists(path))													// если	файл существует
			{
				scintilla1.Text	= File.ReadAllText(path);							// загрузить файл в	скинтиллу

				//scintilla1.Lines[18 -	1].Goto();									// перейти на номер	строки (счёт от	нуля)
				//scintilla1.GotoPosition(scintilla1.Lines[11 -	1].Position);		// перейти на номер	строки (счёт от	нуля)

				//scintilla1.GotoPosition(scintilla1.GetColumn(15 -	1));			// ok
				//scintilla1.GotoPosition(scintilla1.Lines[18 -	1].Position	+ scintilla1.GetColumn(16 -	1));	//


				//scintilla1.GotoPosition(287 -	1);									// от начала текста		;перейти на	колонку	(если такой	нет, возвращает	-1)
				//scintilla1
				//scintilla1.GotoPosition(7	- 1);
				//scintilla1
				//label1.Text =	Convert.ToString(scintilla1.LineFromPosition(288));


				// отметить	(цветом	фона) указанную	строку
				// 'margin[4],[5]' не заняты
				// 'warning' строка
				scintilla1.Margins[4].Type = MarginType.Symbol;
				scintilla1.Markers[4].Symbol = MarkerSymbol.Background;
				scintilla1.Markers[4].SetBackColor(IntToColor(0x1894FE));			// фон 'warning' строки
				// 'error' строка
				scintilla1.Margins[5].Type = MarginType.Symbol;
				scintilla1.Markers[5].Symbol = MarkerSymbol.Background;
				scintilla1.Markers[5].SetBackColor(IntToColor(0xD01010));			// фон 'error' строки
				// отметить	номер строки
				scintilla1.Lines[71	- 1].MarkerAdd(4);								// отметить	71 строку
				scintilla1.Lines[72	- 1].MarkerAdd(5);								// отметить	72 строку
			}


			// Чтение из INI-Файла

			form_top = Convert.ToInt16(INI.Read("Form.Pos",	"Y", "10"));
			form_left =	Convert.ToInt16(INI.Read("Form.Pos", "X", "10"));

			//Form1.ActiveForm.Top = Convert.ToInt16(INI.Read("Position", "Y"));
			//Form1.ActiveForm.Left	= Convert.ToInt16(INI.Read("Position", "X"));

		}



		//---------------------------------------------------------------------
		// Конвертация hex-значения	цвета в	RGB	формат
		//---------------------------------------------------------------------
		public static Color	IntToColor(int rgb)
		{
			return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >>	8),	(byte)rgb);
		}



		//---------------------------------------------------------------------
		// Настройка свойств ScinTilla
		//---------------------------------------------------------------------
		private	void InitSyntaxColoring()
		{
			// Configure the default style
			scintilla1.StyleResetDefault();

			scintilla1.Styles[Style.Default].Font =	"Consolas";
			scintilla1.Styles[Style.Default].Size =	10;

			scintilla1.Styles[Style.Default].BackColor = IntToColor(0x1E1E1E);							// фон тёмный
			scintilla1.Styles[Style.Default].ForeColor = IntToColor(0xDCDCDC);							// цвет	шрифта
			//scintilla1.Styles[Style.Default].BackColor = IntToColor(0xF4F4EB);						  // фон светлый
			//scintilla1.Styles[Style.Default].ForeColor = IntToColor(0x000000);						  // тёмный	цвет шрифта

			// включить	подсветку текущей строки
			scintilla1.CaretLineVisible	= true;
			scintilla1.CaretForeColor =	IntToColor(0xDCDCDC);											// цвет	строки (как	у шрифта)
			scintilla1.CaretLineBackColor =	IntToColor(0x292929);										// фон строки
			//scintilla1.CaretForeColor	= IntToColor(0x000000);											  // тёмный	цвет строки	(как у шрифта)
			//scintilla1.CaretLineBackColor	= IntToColor(0xF4EBEB);										  // светлый фон строки

			// цвет	выделенного	блока текста
			scintilla1.SetSelectionForeColor(false,	IntToColor(0x000000));								// если	выключено, то берётся текущий цвет символов
			scintilla1.SetSelectionBackColor(true, IntToColor(0x264F78));

			// не работает
			// цвета подсветки парных скобок (Brace	Matching)
			scintilla1.Styles[Style.BraceLight].ForeColor =	Color.Blue;	// BlueViolet;
			scintilla1.Styles[Style.BraceLight].BackColor =	Color.Red; // LightGray;
			scintilla1.Styles[Style.BraceBad].ForeColor	= Color.Yellow;
			//scintilla1

			// включить	отображение	верт. линий	отступов (соответствия скобкам)
			scintilla1.IndentationGuides = IndentView.LookBoth;
			scintilla1.IndentationGuides = IndentView.None;												// отключил	(динамически не	отслеживается)



			scintilla1.StyleClearAll();

			//scintilla1.UpdateUI												// обработчик для вывода положения курсора
			//scintilla1.CurrentPosition										// текущая колонка курсора от начала текста	(счёт от нуля)
			//scintilla1.GetColumn(scintilla1.CurrentPosition)					// текущая колонка курсора (счёт от	нуля)
			//scintilla1.CurrentLine											// текущая строка курсора (счёт	от нуля)
			//scintilla1.GotoPosition											// перейти на колонку (если	такой нет, возвращает -1)
			//scintilla1.Lines.Count											// общее кол-во	строк текста
			//scintilla1.CaretLineVisible =													//
			//scintilla1												//
			//scintilla1												//
			//scintilla1												//


			// Configure the CPP (C#) lexer	styles
			scintilla1.Styles[Style.Cpp.Identifier].ForeColor =	IntToColor(0xD0DAE2);
			scintilla1.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
			scintilla1.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
			scintilla1.Styles[Style.Cpp.CommentDoc].ForeColor =	IntToColor(0x2FAE35);
			scintilla1.Styles[Style.Cpp.Number].ForeColor =	IntToColor(0xFFFF00);
			scintilla1.Styles[Style.Cpp.String].ForeColor =	IntToColor(0xFFFF00);
			scintilla1.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
			scintilla1.Styles[Style.Cpp.Preprocessor].ForeColor	= IntToColor(0x8AAFEE);
			scintilla1.Styles[Style.Cpp.Operator].ForeColor	= IntToColor(0xE0E0E0);
			scintilla1.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xFF00FF);
			scintilla1.Styles[Style.Cpp.CommentLineDoc].ForeColor =	IntToColor(0x77A7DB);
			scintilla1.Styles[Style.Cpp.Word].ForeColor	= IntToColor(0x48A8EE);							// 1 вариант
			scintilla1.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);						// 2 вариант
			scintilla1.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
			scintilla1.Styles[Style.Cpp.CommentDocKeywordError].ForeColor =	IntToColor(0xFF0000);
			scintilla1.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

			// тип подсветки
			scintilla1.Lexer = Lexer.Cpp;

			// keywords	(два варианта)
			scintilla1.SetKeywords(0, "class extends implements	import interface new case do while else	if for in switch throw get set function	var	try	catch finally while	with default break continue	delete return each const namespace package include use is as instanceof	typeof author copy default deprecated eventType	example	exampleText	exception haxe inheritDoc internal link	mtasc mxmlc	param private return see serial	serialData serialField since throws	usage version langversion playerversion	productversion dynamic private public partial static intrinsic internal	native override	protected AS3 final	super this arguments null Infinity NaN undefined true false	abstract as	base bool break	by byte	case catch char	checked	class const	continue decimal default delegate do double	descending explicit	event extern else enum false finally fixed float for foreach from goto group if	implicit in	int	interface internal into	is lock	long new null namespace	object operator	out	override orderby params	private	protected public readonly ref return switch	struct sbyte sealed	short sizeof stackalloc	static string select this throw	true try typeof	uint ulong unchecked unsafe	ushort using var virtual volatile void while where yield");
			scintilla1.SetKeywords(1, "void	Null ArgumentError arguments Array Boolean Class Date DefinitionError Error	EvalError Function int Math	Namespace Number Object	RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError	uint XML XMLList Boolean Byte Char DateTime	Decimal	Double Int16 Int32 Int64 IntPtr	SByte Single UInt16	UInt32 UInt64 UIntPtr Void Path	File System	Windows	Forms ScintillaNET");
		}


		//-------------------------------------------------
		// Тест	символа	на скобку(и)
		//-------------------------------------------------
		private	static bool	IsBrace(int	c)
		{
			switch (c)
			{
				case '(':
				case ')':
				case '[':
				case ']':
				case '{':
				case '}':
				//case '<':
				//case '>':
					return true;
			}
			return false;
		}


		//---------------------------------------------------------------------
		// Обработчик события 'UpdateUI'.
		//	- Выводит положение	курсора	(также отрабатывает	от мышки).
		//	- Выводит подсветку	парных скобок.
		//---------------------------------------------------------------------
		private	void scintilla1_UpdateUI(object	sender,	UpdateUIEventArgs e)
		{
			//-------------------------------------------------------
			// вывод в статусбаре положение	курсора
			//-------------------------------------------------------
			toolStripStatusLabel1.Text = "Line:	" +	Convert.ToString(scintilla1.CurrentLine	+ 1) +
										",	Col: " + Convert.ToString(scintilla1.GetColumn(scintilla1.CurrentPosition) + 1);


			//-------------------------------------------------------
			// подсветка парных	скобок
			//-------------------------------------------------------
			// Has the caret changed position?
			var	caretPos = scintilla1.CurrentPosition;
			if (lastCaretPos !=	caretPos)
			{
				lastCaretPos = caretPos;
				var	bracePos1 =	-1;
				var	bracePos2 =	-1;

				// Is there	a brace	to the left	or right?
				if (caretPos > 0 &&	IsBrace(scintilla1.GetCharAt(caretPos -	1)))
						bracePos1 =	(caretPos -	1);
				else if	(IsBrace(scintilla1.GetCharAt(caretPos)))
							bracePos1 =	caretPos;

				if (bracePos1 >= 0)
				{
					// Find	the	matching brace
					bracePos2 =	scintilla1.BraceMatch(bracePos1);
					if (bracePos2 == Scintilla.InvalidPosition)
					{
						scintilla1.BraceBadLight(bracePos1);
						scintilla1.HighlightGuide =	0;														// верт. линии отступов
					}
					else
					{
						scintilla1.BraceHighlight(bracePos1, bracePos2);									// подсветить этим методом две позиции скобки
						scintilla1.HighlightGuide =	scintilla1.GetColumn(bracePos1);						// верт. линии отступов
					}
				}
				else
				{
					// Turn	off	brace matching
					scintilla1.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);		// подсветить этим методом не парную скобку	?
					scintilla1.HighlightGuide =	0;															// верт. линии отступов
				}
			}


		} //scintilla1_UpdateUI





		// цвет	области	номеров	строк
		private	const int FORE_COLOR = 0x2B91AF;															// цвет	visual studio 2019
		private	const int BACK_COLOR = 0x1E1E1F;

		// цвета области 'Code Folding'
		private	const int FOLDING_FORE_COLOR = 0x808080;
		private	const int FOLDING_BACK_COLOR = 0x333333;

		// change this to whatever margin you want the line	numbers	to show	in
		private	const int NUMBER_MARGIN	= 1;

		// change this to whatever margin you want the bookmarks/breakpoints to	show in
		private	const int BOOKMARK_MARGIN =	2;
		private	const int BOOKMARK_MARKER =	2;

		// change this to whatever margin you want the code	folding	tree (+/-) to show in
		private	const int FOLDING_MARGIN = 3;

		// set this	true to	show circular buttons for code folding (the	[+]	and	[-]	buttons	on the margin)
		//private const	bool CODEFOLDING_CIRCULAR =	true;					// вид кружок
		private	const bool CODEFOLDING_CIRCULAR	= false;					// вид квадрат


		private	int	maxLineNumberCharLength	= 0;							// переменная

		//-----------------------------------------------------------
		//	Инициализация поля номеров строк (margin[1])
		//-----------------------------------------------------------
		private	void InitNumberMargin()
		{
			// поле	номеров	строк
			scintilla1.Styles[Style.LineNumber].ForeColor =	IntToColor(FORE_COLOR);
			scintilla1.Styles[Style.LineNumber].BackColor =	IntToColor(BACK_COLOR);

			// отключил
			// верт. линии отступов	(соотв.	скобкам) ?
			scintilla1.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
			scintilla1.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

			// поле	номеров	строк
			var	nums = scintilla1.Margins[NUMBER_MARGIN];
			nums.Width = 30;
			nums.Type =	MarginType.Number;
			nums.Sensitive = true;
			nums.Mask =	0;

			scintilla1.MarginClick += Scintilla1_MarginClick;										// назначить делегат (обработчик события)

		}


		//--- Обработчик события клика в области Margin	(установка Bookmark-ов)
		private	void Scintilla1_MarginClick(object sender, MarginClickEventArgs	e)
		{
			if (e.Margin ==	BOOKMARK_MARGIN)
			{
				// Do we have a	marker for this	line?
				const uint mask	= (1 <<	BOOKMARK_MARKER);
				var	line = scintilla1.Lines[scintilla1.LineFromPosition(e.Position)];

				if ((line.MarkerGet() &	mask) >	0)
				{
					// Remove existing Bookmark
					line.MarkerDelete(BOOKMARK_MARKER);
				}
				else
				{
					// Add Bookmark
					line.MarkerAdd(BOOKMARK_MARKER);
				}
			}
		}


		//-----------------------------------------------------------
		//	Инициализация поля Bookmarks
		//-----------------------------------------------------------
		private	void InitBookmarkMargin()
		{
			//TextArea.SetFoldMarginColor(true,	IntToColor(BACK_COLOR));

			var	margin = scintilla1.Margins[BOOKMARK_MARGIN];
			margin.Width = 17;									// (20)	ширина поля
			margin.Sensitive = true;							// чувствительность	ко клику мышкой
			margin.Type	= MarginType.Symbol;
			margin.Mask	= (1 <<	BOOKMARK_MARKER);
			//margin.Cursor	= MarginCursor.Arrow;

			var	marker = scintilla1.Markers[BOOKMARK_MARKER];
			//marker.Symbol	= MarkerSymbol.Background;			// отметка строки на всю длину
			//marker.Symbol	= MarkerSymbol.Circle;				// вид кружок
			marker.Symbol =	MarkerSymbol.Bookmark;				// вид закладка

			// определение для картинки
			//marker.Symbol	= MarkerSymbol.RgbaImage;
			//marker.DefineRgbaImage(new Bitmap(imageList1.Images[0]));		// одна	картинка

			marker.SetBackColor(IntToColor(0xFF003B));			// тёмно-красная закладка
			//marker.SetBackColor(IntToColor(0x00FF25));		// салатовая закладка
			marker.SetForeColor(IntToColor(0x000000));
			marker.SetAlpha(100);								// непрозрачность		(не	заметил	влияния)
		}


		//-----------------------------------------------------------
		//	Инициализация поля 'Code Folding'
		//-----------------------------------------------------------
		private	void InitCodeFolding()
		{
			scintilla1.SetFoldMarginColor(true,	IntToColor(FOLDING_BACK_COLOR));
			scintilla1.SetFoldMarginHighlightColor(true, IntToColor(FOLDING_BACK_COLOR));

			// Enable code folding
			scintilla1.SetProperty("fold", "1");
			scintilla1.SetProperty("fold.compact", "1");

			// Configure a margin to display folding symbols
			scintilla1.Margins[FOLDING_MARGIN].Type	= MarginType.Symbol;
			scintilla1.Margins[FOLDING_MARGIN].Mask	= Marker.MaskFolders;
			scintilla1.Margins[FOLDING_MARGIN].Sensitive = true;
			scintilla1.Margins[FOLDING_MARGIN].Width = 12;									// ширина поля для маленького квадратика
			//scintilla1.Margins[FOLDING_MARGIN].Width = 18;								// ширина поля для большого	квадратика

			// Set colors for all folding markers
			for	(int i = 25; i <= 31; i++)
			{
				scintilla1.Markers[i].SetForeColor(IntToColor(FOLDING_BACK_COLOR));
				scintilla1.Markers[i].SetBackColor(IntToColor(FOLDING_FORE_COLOR));
			}

			// настройка и отслеживание	вида меток '[+]' и '[-]'
			// Configure folding markers with respective symbols
			scintilla1.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR	? MarkerSymbol.CirclePlus :	MarkerSymbol.BoxPlus;
			scintilla1.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR	? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
			scintilla1.Markers[Marker.FolderEnd].Symbol	= CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected :	MarkerSymbol.BoxPlusConnected;
			scintilla1.Markers[Marker.FolderMidTail].Symbol	= MarkerSymbol.TCorner;
			scintilla1.Markers[Marker.FolderOpenMid].Symbol	= CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
			scintilla1.Markers[Marker.FolderSub].Symbol	= MarkerSymbol.VLine;
			scintilla1.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

			// Enable automatic	folding
			scintilla1.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
		}



		//-------------------------------------------------
		// Кнопка 'Вперёд' на след.	Bookmark
		//-------------------------------------------------
		private	void button1_Click(object sender, EventArgs	e)
		{
			var	line = scintilla1.LineFromPosition(scintilla1.CurrentPosition);
			var	nextLine = scintilla1.Lines[++line].MarkerNext(1 <<	BOOKMARK_MARKER);
			if (nextLine !=	-1)
				scintilla1.Lines[nextLine].Goto();

			scintilla1.Focus();																// сделать видимым курсор
		}

		//-------------------------------------------------
		// Кнопка 'Назад' на пред. Bookmark
		//-------------------------------------------------
		private	void button2_Click(object sender, EventArgs	e)
		{
			var	line = scintilla1.LineFromPosition(scintilla1.CurrentPosition);
			var	prevLine = scintilla1.Lines[--line].MarkerPrevious(1 <<	BOOKMARK_MARKER);
			if (prevLine !=	-1)
				scintilla1.Lines[prevLine].Goto();

			scintilla1.Focus();																// сделать видимым курсор
		}


		//-------------------------------------------------------------------------------
		// Отслеживание	авто-ширины	поля вывода	номеров	строк в	зависимости	от их кол-ва
		// (при	вставке	новой строки)
		//-------------------------------------------------------------------------------
		private	void scintilla1_Insert(object sender, ModificationEventArgs	e)
		{
			//-----	расчёт ширины поля от макс.	номера строки -----------------

			// Did the number of characters	in the line	number display change?
			var	maxLinesCharLength = scintilla1.Lines.Count.ToString().Length;

			if (maxLinesCharLength == this.maxLineNumberCharLength)
				return;

			// Calculate the width required	to display the last	line number	and	include	some padding for good measure.
			const int padding =	2;																// доп.	отступ

			// Margins[x] =	это	индекс поля	вывода номеров строк
			scintilla1.Margins[NUMBER_MARGIN].Width	= scintilla1.TextWidth(Style.LineNumber, new string('9', maxLinesCharLength	+ 1)) +	padding;

			this.maxLineNumberCharLength = maxLinesCharLength;
			//-----------------------------------------------------------------
		}


		// ОТСЛЕДИТЬ УДАЛЕНИЕ ПОМЕЧЕННОЙ СТРОКИ	(УБРАТЬ	ИЗ СПИСКА ПОМЕЧЕННЫХ)
		//-------------------------------------------------------------------------------
		// Отслеживание	авто-ширины	поля вывода	номеров	строк в	зависимости	от их кол-ва
		// (при	удалении строки)
		//-------------------------------------------------------------------------------
		private	void scintilla1_Delete(object sender, ModificationEventArgs	e)
		{
			//-----	расчёт ширины поля от макс.	номера строки -----------------

			// Did the number of characters	in the line	number display change?
			var	maxLinesCharLength = scintilla1.Lines.Count.ToString().Length;

			if (maxLinesCharLength == this.maxLineNumberCharLength)
				return;

			// Calculate the width required	to display the last	line number	and	include	some padding for good measure.
			const int padding =	2;																		// доп.	отступ

			// Margins[x] =	это	индекс поля	вывода номеров строк
			scintilla1.Margins[NUMBER_MARGIN].Width	= scintilla1.TextWidth(Style.LineNumber, new string('9', maxLinesCharLength	+ 1)) +	padding;

			this.maxLineNumberCharLength = maxLinesCharLength;
			//-----------------------------------------------------------------
		}

		private	void button3_Click(object sender, EventArgs	e)
		{
			label2.Text	= Convert.ToString(scintilla1.FirstVisibleLine + 1);							// номер верхней строки	(от	нуля)
			label3.Text	= Convert.ToString(scintilla1.FirstVisibleLine + scintilla1.LinesOnScreen);		// номер нижней	строки
			label4.Text	= Convert.ToString(scintilla1.LinesOnScreen);									// кол-во строк	в окне редактора

			var	line = 100;							// нужная строка

			//var line = scintilla1.LineFromPosition(scintilla1.CurrentPosition);
			//			var	line1 =	line; //
			//			var	nextLine = scintilla1.Lines[++line].MarkerNext(1 <<	BOOKMARK_MARKER);
			//			if (nextLine !=	-1)
			//			{
			//				scintilla1.Lines[nextLine].Goto();											// перейти на строку

			// нужная строка уже на	экране,	попытка	её отцентровать	в середину экрана
			//					var	start =	scintilla1.Lines[line -	(scintilla1.LinesOnScreen /	2)].Position;
			//					var	end	= scintilla1.Lines[line	+ (scintilla1.LinesOnScreen	/ 2)].Position;

//			var	start =	(line -	(scintilla1.LinesOnScreen /	2));
//			var	end	= (line	+ (scintilla1.LinesOnScreen	/ 2));
			var	start =	scintilla1.Lines[line -	(scintilla1.LinesOnScreen /	2)].Position;
			var	end	= scintilla1.Lines[line	+ (scintilla1.LinesOnScreen	/ 2)].Position;

			//var line = scintilla1.LineFromPosition(scintilla1.CurrentPosition);

			//label5.Text =	Convert.ToString(line -	(scintilla1.FirstVisibleLine + 1));
			//label6.Text =	Convert.ToString(line +	(scintilla1.FirstVisibleLine + scintilla1.LinesOnScreen));
			label5.Text	= Convert.ToString(start);// (line - (scintilla1.LinesOnScreen / 2));
			label6.Text	= Convert.ToString(end);// (line + (scintilla1.LinesOnScreen / 2));

			//label5.Text =	Convert.ToString(scintilla1.LineFromPosition(scintilla1.FirstVisibleLine + 1));							// номер верхней строки (от нуля)
			//label6.Text =	Convert.ToString(scintilla1.LineFromPosition(scintilla1.FirstVisibleLine + scintilla1.LinesOnScreen));	// номер нижней строки
			//label5.Text =	Convert.ToString(scintilla1.CurrentPosition);//	(scintilla1.FirstVisibleLine + 1));						// номер верхней строки (от нуля)
			//label6.Text =	Convert.ToString(scintilla1.CurrentPosition);//	(scintilla1.FirstVisibleLine + scintilla1.LinesOnScreen));	// номер нижней строки



			scintilla1.ScrollRange(start, end);											// отцентровать	(по-возможности)
			//scintilla1.GotoPosition(end -	start);
			
			//scintilla1

		}


		//---------------------------------------------------------------------
		// Событие при первом отображении формы	приложения
		//---------------------------------------------------------------------
		private	void Form1_Shown(object	sender,	EventArgs e)
		{
			// положение окна приложения
			Form1.ActiveForm.Top = form_top;
			Form1.ActiveForm.Left =	form_left;
		}


		//---------------------------------------------------------------------
		//	Событие	перед закрытием	формы приложения
		//---------------------------------------------------------------------
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			// записать	в INI-файл положение окна приложения
			INI.Write("Form.Pos", "Y", Convert.ToString(Form1.ActiveForm.Top));
			INI.Write("Form.Pos", "X", Convert.ToString(Form1.ActiveForm.Left));
		}



		// Кнопка 'Открыть папку'
		private void button4_Click(object sender, EventArgs e)
		{
			//dlg.InitialDirectory = "C:\\";												// начальный путь (папка)
			opn.IsFolderPicker = true;														// для 'Open Folder'
			if (opn.ShowDialog() == CommonFileDialogResult.Ok) {
				label7.Text = "путь папки: " + opn.FileName;								// Ok
			}
		}


		// Кнопка 'Открыть файл'
		private void button5_Click(object sender, EventArgs e)
		{
			//dlg.InitialDirectory = "C:\\";												// начальный путь (папка)
			opn.IsFolderPicker = false;														// для 'Open File'
			if (opn.ShowDialog() == CommonFileDialogResult.Ok) {
				label7.Text = "путь файла: " + opn.FileName;								// Ok
			}
		}


		// Кнопка 'Сохранить файл'
		private void button6_Click(object sender, EventArgs e)
		{
			sav.Title = "Сохранить мой файл";
			sav.InitialDirectory = "C:\\IAR-Z80";											// настроить папку
			sav.DefaultFileName = "ide.ini";												// настроить имя файла

			if (sav.ShowDialog() == CommonFileDialogResult.Ok) {
				//label7.Text = "файл сохранён: " + sav.FileName;
				label7.Text = "файл сохранён: " + sav.DefaultFileName;
			}
			else {
				label7.Text = "Ошибка !!!";
			}

		}



	}
}
