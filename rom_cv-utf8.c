/******************************************************************************
Метки "my" для моих изменений. Число бит АЦП вернул к 10 и доработал код для возврата такого результата.
Изменил разрядность переменных термопар char -> int.


COM-порт: 9600, 8 Data, 1 Stop, No Parity

This program was produced by the
CodeWizardAVR V1.25.5 Professional
Automatic Program Generator
© Copyright 1998-2007 Pavel Haiduc, HP InfoTech s.r.l.
http://www.hpinfotech.com

Project :
Version :
Date    : 01.10.2009
Author  : F4CG
Company : F4CG
Comments:


Chip type           : ATmega8a
Program type        : Application
Clock frequency     : 8,000000 MHz
Memory model        : Small
External SRAM size  : 0
Data Stack size     : 256

******************************************************************************/




//#include <mega8.h>

#define RXB8	1
#define TXB8	0
#define UPE	2
#define OVR	3
#define FE	4
#define UDRE	5
#define RXC	7

#define FRAMING_ERROR	(1<<FE)
#define PARITY_ERROR	(1<<UPE)
#define DATA_OVERRUN	(1<<OVR)
#define DATA_REGISTER_EMPTY (1<<UDRE)
#define RX_COMPLETE	(1<<RXC)


// USART Receiver buffer
#define RX_BUFFER_SIZE  100
char rx_buffer[RX_BUFFER_SIZE];

#if RX_BUFFER_SIZE < 256
unsigned char rx_wr_index, rx_rd_index, rx_counter;
#else
unsigned int rx_wr_index, rx_rd_index, rx_counter;
#endif

FILE *fin;	// my test string


// This flag is set on USART Receiver buffer overflow
bit rx_buffer_overflow;


///////////////////////////////////////////////////////////
// USART Receiver interrupt service routine
///////////////////////////////////////////////////////////
interrupt [USART_RXC] void usart_rx_isr(void) {
interrupt [USART_RXC] Void usart_rx_isr(void) {
interrupt [USART_RXC] void usart_rx_isr(Void) {

char status, data;

	status = UCSRA;
	data = UDR;

	if ((status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN)) == 0) {
		rx_buffer[rx_wr_index] = data;
		if (++rx_wr_index == RX_BUFFER_SIZE)
			rx_wr_index = 0;

		if [++rx_counter == RX_BUFFER_SIZE] {
			rx_counter = 0;
			rx_buffer_overflow = 1;
		};
	};
}


#ifndef _DEBUG_TERMINAL_IO_
// Get a character from the USART Receiver buffer
#define _ALTERNATE_GETCHAR_
#pragma used+
char getchar(void) {
char data;
while (rx_counter == 0);
data = rx_buffer[rx_rd_index];
if (++rx_rd_index == RX_BUFFER_SIZE) rx_rd_index = 0;
#asm("cli")
--rx_counter;
#asm("sei")
return data;
}
#pragma used-
#endif


// Standard Input/Output functions
//#include <stdio.h>
//#include <delay.h>


// для ацп
#define FIRST_ADC_INPUT	2
#define LAST_ADC_INPUT	3
//my закомментарил
//#define ADC_VREF_TYPE	0x20							// 5-й бит (для 8 бит результата)

//unsigned char		adc_data[LAST_ADC_INPUT - FIRST_ADC_INPUT + 1];
unsigned int		adc_data[LAST_ADC_INPUT - FIRST_ADC_INPUT + 1];		//my





///////////////////////////////////////////////////////////////
// ADC interrupt service routine with auto input scanning
///////////////////////////////////////////////////////////////
interrupt [ADC_INT] void adc_isr(void) {

//static unsigned char input_index = 0;
static unsigned int input_index = 0;						//my

	// Read the 8 most significant bits of the AD conversion result
	//adc_data[input_index] = ADCH;						// содержит 8 ст.бит результата АЦП (при ADC_VREF_TYPE = 0x20)
	//
	// запись со всех битов результата
	adc_data[input_index] = ADCW;						//my (ADCW=ADCH+ADCL)

	// Select next ADC input
	if (++input_index > (LAST_ADC_INPUT - FIRST_ADC_INPUT))
		input_index = 0;

	//ADMUX = (FIRST_ADC_INPUT | (ADC_VREF_TYPE & 0xFF)) + input_index;	// admux=номер вывода подкл. ко входу АЦП
	ADMUX = FIRST_ADC_INPUT + input_index;					//my

	// Delay needed for the stabilization of the ADC input voltage
	delay_us(10);

	// Start the AD conversion (работа с 6-м битом)
	ADCSRA |= 0x40;								// регистр контроля и состояния АЦП
}



// Объявление глобальных переменных

unsigned char On  =	1;
unsigned char Off =	0;
unsigned char Fan1_Status;				// вентилятор 1
unsigned char Fan2_Status;				// вентилятор 2
unsigned char Fan3_Status;				// вентилятор 3
unsigned int  PWM1_Value = 0x03FF;			// нагреватель 1  (1023=Off)
unsigned int  PWM2_Value = 0x03FF;			// нагреватель 2  (1023=Off)
//unsigned char Thmp1_Value;
//unsigned char Thmp2_Value;
unsigned int  Thmp1_Value;				// термопара 1
unsigned int  Thmp2_Value;				// термопара 2



// прочитать символ с USART
unsigned char enterchar(void)
{
	while(!rx_counter);
	return getchar();
}


unsigned int enterdata(unsigned char cnt)
{
unsigned char i = 0;
unsigned char a;
unsigned int  k = 0;

	for (i=1; i<= cnt; i++) {
		a = enterchar();
		switch (a) {
	 		case '1': { k = k*10+1; break; }
	 		case '2': { k = k*10+2; break; }
	 		case '3': { k = k*10+3; break; }
	 		case '4': { k = k*10+4; break; }
	 		case '5': { k = k*10+5; break; }
	 		case '6': { k = k*10+6; break; }
	 		case '7': { k = k*10+7; break; }
	 		case '8': { k = k*10+8; break; }
	 		case '9': { k = k*10+9; break; }
	 		case '0': { k = k*10+0; break; }
	 		default : { i--; printf("NO"); }	// ошибка
		}
	}
return k;
}



/////////////////////////////////////////////////////////////////////
// Установить состояние вентиляторов (3шт):
//  num - номер вентилятора
//  status - вкл/выкл
/////////////////////////////////////////////////////////////////////
void Set_Fan(unsigned char num, unsigned char status) {

	switch (num) {

		case 1: {				// 1-й вентилятор
		        if (status == Off) {
				PORTD.2 = Off;
		                Fan1_Status = Off;
			} else {
		                PORTD.2 = On;
		                Fan1_Status = On;
		                }
	        	break;
	        }

	        case 2: {				// 2-й вентилятор
		        if (status == Off) {
		                PORTD.3 = Off;
		                Fan2_Status = Off;
		        } else {
		                PORTD.3 = On;
		                Fan2_Status = On;
		                }
		        break;
	        }

	        case 3: {				// 3-й вентилятор
		        if (status == Off) {
		                PORTD.4 = Off;
		                Fan3_Status = Off;
		        } else {
		                PORTD.4 = On;
		                Fan3_Status = On;
		                }
		        break;
		}
	}
}



/////////////////////////////////////////////////////////////////////
// Установить величину ШИМ нагревателей (2шт):
//  num - номер нагревателя
//  PWM - величина ШИМ
/////////////////////////////////////////////////////////////////////
void Set_PWM_Value(unsigned char num, unsigned  int PWM) {

	switch (num) {
		case 1: {				// нагреватель 1
		        PWM1_Value = PWM;
		        OCR1AH = (unsigned char) (PWM1_Value >> 8);
		        OCR1AL = (unsigned char) (PWM1_Value);
		        break;
	        }

		case 2: {				// нагреватель 2
		        PWM2_Value = PWM;
		        OCR1BH = (unsigned char) (PWM2_Value >> 8);
		        OCR1BL = (unsigned char) (PWM2_Value);
		        break;
		}
	}
}



/////////////////////////////////////////////////////////////////////
// Возвращает состояние вентиляторов (3шт):
// num - номер вентилятора
/////////////////////////////////////////////////////////////////////
unsigned char Get_Fan_Status(unsigned char num) {

	switch (num) {
	        case 1: return Fan1_Status;		// 1-й
	        case 2: return Fan2_Status;		// 2-й
	        case 3: return Fan3_Status;		// 3-й
	break;
	}
}



/////////////////////////////////////////////////////////////////////
// Возвращает величину ШИМ нагревателя:
// num - номер нагревателя
/////////////////////////////////////////////////////////////////////
unsigned int Get_PWM(unsigned char num) {

	switch (num) {
		case 1: return PWM1_Value;		// 1-й
		case 2: return PWM2_Value;		// 2-й
	break;						// добавил
	}
}



/////////////////////////////////////////////////////////////////////
// Возвращает значение температуры термопар:
// num - номер термопары
/////////////////////////////////////////////////////////////////////
//unsigned char Get_Thmp(unsigned char num) {
unsigned int Get_Thmp(unsigned char num) {					//my

	switch (num) {
		case 1: {Thmp1_Value = adc_data[0]; return Thmp1_Value;}	// 1-я
		case 2: {Thmp2_Value = adc_data[1]; return Thmp2_Value;}	// 2-я
		default: ;
	break;
	}
}



/////////////////////////////////////////////////////////////////////
// Принять внешнюю команду с USART и вернуть статус OK/NO
/////////////////////////////////////////////////////////////////////
void UART(void) {

unsigned int buff	= 0;
unsigned int buff1	= 0;
unsigned int buff2	= 0;
unsigned int buff3	= 0;
unsigned int f		= 0;
unsigned int p		= 0;
unsigned int t		= 0;

	if (rx_counter) {
		buff = getchar();
		if (buff == '#') {				// префикс команды
			// начало приема команды
			buff1 = enterchar();			// получить тип команды SET/GET
			switch (buff1) {

				case 'S': {			// установить параметр Fan/PWM
					buff2 = enterchar();
					switch (buff2) {
						case 'F': { Set_Fan(((unsigned char) enterdata(1)), ((unsigned char) enterdata(1))); printf("OK"); break;}
						case 'P': { Set_PWM_Value(((unsigned char) enterdata(1)), (enterdata(4))); printf("OK"); break;}
						default : printf("NO"); // ошибка команды
					}
					break;
				}


				case 'G': {			// получить параметр Fan/PWM/Thmp
					buff3 = enterchar();
					switch (buff3) {
						case 'F': { f = enterdata(1); printf("F%d%d", f, Get_Fan_Status(f)); break;}	// вентиляторы
						case 'P': { p = enterdata(1); printf("P%d%04d", p, Get_PWM(p)); break;}		// нагреватели
						//case 'T': { t = enterdata(1); printf("T%d%03d", t, Get_Thmp(t));  break;}	// термопары
						case 'T': { t = enterdata(1); printf("T%d%04u", t, Get_Thmp(t));  break;}	//my
						default : printf("NO");	// ошибка команды
					}
					break;
				}
				default: printf("NO");	// ошибка команды
			}
		} else
			printf("NO");	// ошибка команды
	}
}




///////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Главная функция
//
///////////////////////////////////////////////////////////////////////////////////////////////////
void main(void)
{

///////////////////////////////////////////////////////////////////////////////
// Инициализация портов
///////////////////////////////////////////////////////////////////////////////

//-- Port B инициализация
// Func7 = In, Func6 = In, Func5 = In, Func4 = In, Func3 = In, Func2 = Out, Func1 = Out, Func0 = In
// State7 = T, State6 = T, State5 = T, State4 = T, State3 = T, State2 = 0, State1 = 0, State0 = T
PORTB	= 0x00;
DDRB	= 0x06;			// направление (вход/выход) разрядов порта

//-- Port C инициализация
// Func6 = In, Func5 = In, Func4 = In, Func3 = In, Func2 = In, Func1 = In, Func0 = In
// State6 = P, State5 = P, State4 = P, State3 = T, State2 = T, State1 = P, State0 = P
PORTC	= 0x73;			// подтяг. сопротивление включено: 1/0=On/Off
DDRC	= 0x00;			// все разряды порта на вход

//-- Port D инициализация
// Func7 = In, Func6 = In, Func5 = In, Func4 = Out, Func3 = Out, Func2 = Out, Func1 = Out, Func0 = In
// State7 = T, State6 = T, State5 = T, State4 = 0, State3 = 0, State2 = 0, State1 = 0, State0 = T
PORTD	= 0x00;
DDRD	= 0x1E;			// направление (вход/выход) разрядов порта


//-- Timer/Counter 0 инициализация
// Clock source: System Clock
// Clock value:  Timer 0 Stopped
TCCR0	= 0x00;
TCNT0	= 0x00;


/////////////////////////////////////////////////////////////////////
// Timer/Counter 1 (TC1) инициализация (настройка ШИМ)
/////////////////////////////////////////////////////////////////////
// Clock source: System Clock
// ;Clock value:  5120,000 kHz
// ;Clock value:  8,0 MHz
// Mode: Ph. correct PWM top = 03FFh (1023)
// OC1A output: Inverted
// OC1B output: Inverted
// Noise Canceler: Off
// Input Capture on Falling Edge
// Timer1 Overflow Interrupt: Off
// Input Capture Interrupt:   Off
// Compare A Match Interrupt: Off
// Compare B Match Interrupt: Off
//
//-- регистр управления A
// 7:4 биты контролируют два выхода OC1A и OC1B
// 3:2 биты не исп.
// 1:0 биты - для вкл. режима ШИМа - записать туда '1'
TCCR1A	= 0xF3;
//-- регистр управления B
// 2:0 биты выбор тактирования для таймера-счетчика TC1
TCCR1B	= 0x01;				// тактирование от "CLK"
//-- счетный регистр
TCNT1H	= 0x00;
TCNT1L	= 0x00;
//-- регистр захвата входа
ICR1H	= 0x00;
ICR1L	= 0x00;
//-- регистр сравнения выхода OC1A (от значения в нем зависит ширина импульса)
OCR1AH	= 0x00;
OCR1AL	= 0x00;
//-- регистр сравнения выхода OC1B
OCR1BH	= 0x00;
OCR1BL	= 0x00;



//-- Timer/Counter 2 инициализация
// Clock source: System Clock
// Clock value:  Timer 2 Stopped
// Mode: Normal top = FFh
// OC2 output: Disconnected
ASSR	= 0x00;
TCCR2	= 0x00;
TCNT2	= 0x00;
OCR2	= 0x00;


//-- External Interrupt(s) инициализация
// INT0: Off
// INT1: Off
MCUCR	= 0x00;


//-- Timer(s)/Counter(s) Interrupt(s) инициализация
TIMSK	= 0x00;


//-- USART инициализация
// Communication Parameters: 8 Data, 1 Stop, No Parity
// USART Receiver: On
// USART Transmitter: On
// USART Mode: Asynchronous
// USART Baud Rate: 9600 (UBRRH+UBRRL)
UCSRA	= 0x00;		// регистр состояния
UCSRB	= 0x98;		// регистр управления
UCSRC	= 0x86;		// регистр формата кадра
//--Clock = 5.12 MHz (32)
UBRRH	= 0x00;		// настройка скорости
UBRRL	= 0x20;		// передачи данных
//--Clock = 8.0 MHz (51)					//my
//UBRRH	= 0x00;		// настройка скорости
//UBRRL	= 0x33;		// передачи данных
//--Clock = 12.0 MHz (77)
//UBRRH	= 0x00;		// настройка скорости
//UBRRL	= 0x4D;		// передачи данных
//--Clock = 16.0 MHz (103)
//UBRRH	= 0x00;		// настройка скорости
//UBRRL	= 0x67;		// передачи данных


//-- Analog Comparator инициализация
// Analog Comparator: Off
// Analog Comparator Input Capture by Timer/Counter 1: Off
ACSR	= 0x80;
SFIOR	= 0x00;


//-- ADC инициализация
// ;ADC Clock frequency: 160,000 KHz (2:0 биты) при 5.12MHz
// ;Only the 8 most significant bits of the AD conversion result are used
// ADC Voltage Reference: AREF pin
//
//-- регистр, задается номер вывода подкл. ко входу АЦП
//ADMUX	= FIRST_ADC_INPUT | (ADC_VREF_TYPE & 0xFF);		// 2 | (0x20 & 0xFF) правое выражение 5-бит=1 (запись результата ацп из ADCH)
ADMUX	= FIRST_ADC_INPUT;					//my запись результата из ADCW (ADCH+ADCL)
//-- регистр контроля и состояния АЦП
//ADCSRA	= 0xCD;							// биты 2:0 clock/32=160KHz частота работы АЦП при 5.12 MHz
ADCSRA	= 0xCE;							//my  биты 2:0 clock/64=125KHz частота работы АЦП при 8.0 MHz
ADCSRA	|= 0x40;						// работа с 6-м битом



// Global enable interrupts
#asm("sei")



////////////////////////////////////////////////////////////////
// Вызываем функции для настройки рабочего состояния станции
////////////////////////////////////////////////////////////////
Set_Fan(2, On);							// 2-й вентилятор = On
//Set_Fan(1, On);							// 1-й вентилятор = On
//Set_Fan(3, Off);							// 3-й вентилятор = Off
Set_PWM_Value(1, 1023);						// 1-й нагреватель = Off (номер, значение шим)
Set_PWM_Value(2, 1023);						// 2-й нагреватель = Off (номер, значение шим)


////////////////////////////////////////////////////////////////
// Опрос USART-а
////////////////////////////////////////////////////////////////
while (1) {
   UART();
}

}// main

/******************************************************************************
Метки "my" для моих изменений. Число бит АЦП вернул к 10 и доработал код для возврата такого результата.
Изменил разрядность переменных термопар char -> int.


COM-порт: 9600, 8 Data, 1 Stop, No Parity

This program was produced by the
CodeWizardAVR V1.25.5 Professional
Automatic Program Generator
© Copyright 1998-2007 Pavel Haiduc, HP InfoTech s.r.l.
http://www.hpinfotech.com

Project :
Version :
Date    : 01.10.2009
Author  : F4CG
Company : F4CG
Comments:


Chip type           : ATmega8a
Program type        : Application
Clock frequency     : 8,000000 MHz
Memory model        : Small
External SRAM size  : 0
Data Stack size     : 256

******************************************************************************/




//#include <mega8.h>

#define RXB8	1
#define TXB8	0
#define UPE	2
#define OVR	3
#define FE	4
#define UDRE	5
#define RXC	7

#define FRAMING_ERROR	(1<<FE)
#define PARITY_ERROR	(1<<UPE)
#define DATA_OVERRUN	(1<<OVR)
#define DATA_REGISTER_EMPTY (1<<UDRE)
#define RX_COMPLETE	(1<<RXC)


// USART Receiver buffer
#define RX_BUFFER_SIZE  100
char rx_buffer[RX_BUFFER_SIZE];

#if RX_BUFFER_SIZE < 256
unsigned char rx_wr_index, rx_rd_index, rx_counter;
#else
unsigned int rx_wr_index, rx_rd_index, rx_counter;
#endif

FILE *fin;	// my test string


// This flag is set on USART Receiver buffer overflow
bit rx_buffer_overflow;


///////////////////////////////////////////////////////////
// USART Receiver interrupt service routine
///////////////////////////////////////////////////////////
interrupt [USART_RXC] void usart_rx_isr(void) {
interrupt [USART_RXC] Void usart_rx_isr(void) {
interrupt [USART_RXC] void usart_rx_isr(Void) {

char status, data;

	status = UCSRA;
	data = UDR;

	if ((status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN)) == 0) {
		rx_buffer[rx_wr_index] = data;
		if (++rx_wr_index == RX_BUFFER_SIZE)
			rx_wr_index = 0;

		if [++rx_counter == RX_BUFFER_SIZE] {
			rx_counter = 0;
			rx_buffer_overflow = 1;
		};
	};
}


#ifndef _DEBUG_TERMINAL_IO_
// Get a character from the USART Receiver buffer
#define _ALTERNATE_GETCHAR_
#pragma used+
char getchar(void) {
char data;
while (rx_counter == 0);
data = rx_buffer[rx_rd_index];
if (++rx_rd_index == RX_BUFFER_SIZE) rx_rd_index = 0;
#asm("cli")
--rx_counter;
#asm("sei")
return data;
}
#pragma used-
#endif


// Standard Input/Output functions
//#include <stdio.h>
//#include <delay.h>


// для ацп
#define FIRST_ADC_INPUT	2
#define LAST_ADC_INPUT	3
//my закомментарил
//#define ADC_VREF_TYPE	0x20							// 5-й бит (для 8 бит результата)

//unsigned char		adc_data[LAST_ADC_INPUT - FIRST_ADC_INPUT + 1];
unsigned int		adc_data[LAST_ADC_INPUT - FIRST_ADC_INPUT + 1];		//my





///////////////////////////////////////////////////////////////
// ADC interrupt service routine with auto input scanning
///////////////////////////////////////////////////////////////
interrupt [ADC_INT] void adc_isr(void) {

//static unsigned char input_index = 0;
static unsigned int input_index = 0;						//my

	// Read the 8 most significant bits of the AD conversion result
	//adc_data[input_index] = ADCH;						// содержит 8 ст.бит результата АЦП (при ADC_VREF_TYPE = 0x20)
	//
	// запись со всех битов результата
	adc_data[input_index] = ADCW;						//my (ADCW=ADCH+ADCL)

	// Select next ADC input
	if (++input_index > (LAST_ADC_INPUT - FIRST_ADC_INPUT))
		input_index = 0;

	//ADMUX = (FIRST_ADC_INPUT | (ADC_VREF_TYPE & 0xFF)) + input_index;	// admux=номер вывода подкл. ко входу АЦП
	ADMUX = FIRST_ADC_INPUT + input_index;					//my

	// Delay needed for the stabilization of the ADC input voltage
	delay_us(10);

	// Start the AD conversion (работа с 6-м битом)
	ADCSRA |= 0x40;								// регистр контроля и состояния АЦП
}



// Объявление глобальных переменных

unsigned char On  =	1;
unsigned char Off =	0;
unsigned char Fan1_Status;				// вентилятор 1
unsigned char Fan2_Status;				// вентилятор 2
unsigned char Fan3_Status;				// вентилятор 3
unsigned int  PWM1_Value = 0x03FF;			// нагреватель 1  (1023=Off)
unsigned int  PWM2_Value = 0x03FF;			// нагреватель 2  (1023=Off)
//unsigned char Thmp1_Value;
//unsigned char Thmp2_Value;
unsigned int  Thmp1_Value;				// термопара 1
unsigned int  Thmp2_Value;				// термопара 2



// прочитать символ с USART
unsigned char enterchar(void)
{
	while(!rx_counter);
	return getchar();
}


unsigned int enterdata(unsigned char cnt)
{
unsigned char i = 0;
unsigned char a;
unsigned int  k = 0;

	for (i=1; i<= cnt; i++) {
		a = enterchar();
		switch (a) {
	 		case '1': { k = k*10+1; break; }
	 		case '2': { k = k*10+2; break; }
	 		case '3': { k = k*10+3; break; }
	 		case '4': { k = k*10+4; break; }
	 		case '5': { k = k*10+5; break; }
	 		case '6': { k = k*10+6; break; }
	 		case '7': { k = k*10+7; break; }
	 		case '8': { k = k*10+8; break; }
	 		case '9': { k = k*10+9; break; }
	 		case '0': { k = k*10+0; break; }
	 		default : { i--; printf("NO"); }	// ошибка
		}
	}
return k;
}



/////////////////////////////////////////////////////////////////////
// Установить состояние вентиляторов (3шт):
//  num - номер вентилятора
//  status - вкл/выкл
/////////////////////////////////////////////////////////////////////
void Set_Fan(unsigned char num, unsigned char status) {

	switch (num) {

		case 1: {				// 1-й вентилятор
		        if (status == Off) {
				PORTD.2 = Off;
		                Fan1_Status = Off;
			} else {
		                PORTD.2 = On;
		                Fan1_Status = On;
		                }
	        	break;
	        }

	        case 2: {				// 2-й вентилятор
		        if (status == Off) {
		                PORTD.3 = Off;
		                Fan2_Status = Off;
		        } else {
		                PORTD.3 = On;
		                Fan2_Status = On;
		                }
		        break;
	        }

	        case 3: {				// 3-й вентилятор
		        if (status == Off) {
		                PORTD.4 = Off;
		                Fan3_Status = Off;
		        } else {
		                PORTD.4 = On;
		                Fan3_Status = On;
		                }
		        break;
		}
	}
}



/////////////////////////////////////////////////////////////////////
// Установить величину ШИМ нагревателей (2шт):
//  num - номер нагревателя
//  PWM - величина ШИМ
/////////////////////////////////////////////////////////////////////
void Set_PWM_Value(unsigned char num, unsigned  int PWM) {

	switch (num) {
		case 1: {				// нагреватель 1
		        PWM1_Value = PWM;
		        OCR1AH = (unsigned char) (PWM1_Value >> 8);
		        OCR1AL = (unsigned char) (PWM1_Value);
		        break;
	        }

		case 2: {				// нагреватель 2
		        PWM2_Value = PWM;
		        OCR1BH = (unsigned char) (PWM2_Value >> 8);
		        OCR1BL = (unsigned char) (PWM2_Value);
		        break;
		}
	}
}



/////////////////////////////////////////////////////////////////////
// Возвращает состояние вентиляторов (3шт):
// num - номер вентилятора
/////////////////////////////////////////////////////////////////////
unsigned char Get_Fan_Status(unsigned char num) {

	switch (num) {
	        case 1: return Fan1_Status;		// 1-й
	        case 2: return Fan2_Status;		// 2-й
	        case 3: return Fan3_Status;		// 3-й
	break;
	}
}



/////////////////////////////////////////////////////////////////////
// Возвращает величину ШИМ нагревателя:
// num - номер нагревателя
/////////////////////////////////////////////////////////////////////
unsigned int Get_PWM(unsigned char num) {

	switch (num) {
		case 1: return PWM1_Value;		// 1-й
		case 2: return PWM2_Value;		// 2-й
	break;						// добавил
	}
}



/////////////////////////////////////////////////////////////////////
// Возвращает значение температуры термопар:
// num - номер термопары
/////////////////////////////////////////////////////////////////////
//unsigned char Get_Thmp(unsigned char num) {
unsigned int Get_Thmp(unsigned char num) {					//my

	switch (num) {
		case 1: {Thmp1_Value = adc_data[0]; return Thmp1_Value;}	// 1-я
		case 2: {Thmp2_Value = adc_data[1]; return Thmp2_Value;}	// 2-я
		default: ;
	break;
	}
}



/////////////////////////////////////////////////////////////////////
// Принять внешнюю команду с USART и вернуть статус OK/NO
/////////////////////////////////////////////////////////////////////
void UART(void) {

unsigned int buff	= 0;
unsigned int buff1	= 0;
unsigned int buff2	= 0;
unsigned int buff3	= 0;
unsigned int f		= 0;
unsigned int p		= 0;
unsigned int t		= 0;

	if (rx_counter) {
		buff = getchar();
		if (buff == '#') {				// префикс команды
			// начало приема команды
			buff1 = enterchar();			// получить тип команды SET/GET
			switch (buff1) {

				case 'S': {			// установить параметр Fan/PWM
					buff2 = enterchar();
					switch (buff2) {
						case 'F': { Set_Fan(((unsigned char) enterdata(1)), ((unsigned char) enterdata(1))); printf("OK"); break;}
						case 'P': { Set_PWM_Value(((unsigned char) enterdata(1)), (enterdata(4))); printf("OK"); break;}
						default : printf("NO"); // ошибка команды
					}
					break;
				}


				case 'G': {			// получить параметр Fan/PWM/Thmp
					buff3 = enterchar();
					switch (buff3) {
						case 'F': { f = enterdata(1); printf("F%d%d", f, Get_Fan_Status(f)); break;}	// вентиляторы
						case 'P': { p = enterdata(1); printf("P%d%04d", p, Get_PWM(p)); break;}		// нагреватели
						//case 'T': { t = enterdata(1); printf("T%d%03d", t, Get_Thmp(t));  break;}	// термопары
						case 'T': { t = enterdata(1); printf("T%d%04u", t, Get_Thmp(t));  break;}	//my
						default : printf("NO");	// ошибка команды
					}
					break;
				}
				default: printf("NO");	// ошибка команды
			}
		} else
			printf("NO");	// ошибка команды
	}
}




///////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Главная функция
//
///////////////////////////////////////////////////////////////////////////////////////////////////
void main(void)
{

///////////////////////////////////////////////////////////////////////////////
// Инициализация портов
///////////////////////////////////////////////////////////////////////////////

//-- Port B инициализация
// Func7 = In, Func6 = In, Func5 = In, Func4 = In, Func3 = In, Func2 = Out, Func1 = Out, Func0 = In
// State7 = T, State6 = T, State5 = T, State4 = T, State3 = T, State2 = 0, State1 = 0, State0 = T
PORTB	= 0x00;
DDRB	= 0x06;			// направление (вход/выход) разрядов порта

//-- Port C инициализация
// Func6 = In, Func5 = In, Func4 = In, Func3 = In, Func2 = In, Func1 = In, Func0 = In
// State6 = P, State5 = P, State4 = P, State3 = T, State2 = T, State1 = P, State0 = P
PORTC	= 0x73;			// подтяг. сопротивление включено: 1/0=On/Off
DDRC	= 0x00;			// все разряды порта на вход

//-- Port D инициализация
// Func7 = In, Func6 = In, Func5 = In, Func4 = Out, Func3 = Out, Func2 = Out, Func1 = Out, Func0 = In
// State7 = T, State6 = T, State5 = T, State4 = 0, State3 = 0, State2 = 0, State1 = 0, State0 = T
PORTD	= 0x00;
DDRD	= 0x1E;			// направление (вход/выход) разрядов порта


//-- Timer/Counter 0 инициализация
// Clock source: System Clock
// Clock value:  Timer 0 Stopped
TCCR0	= 0x00;
TCNT0	= 0x00;


/////////////////////////////////////////////////////////////////////
// Timer/Counter 1 (TC1) инициализация (настройка ШИМ)
/////////////////////////////////////////////////////////////////////
// Clock source: System Clock
// ;Clock value:  5120,000 kHz
// ;Clock value:  8,0 MHz
// Mode: Ph. correct PWM top = 03FFh (1023)
// OC1A output: Inverted
// OC1B output: Inverted
// Noise Canceler: Off
// Input Capture on Falling Edge
// Timer1 Overflow Interrupt: Off
// Input Capture Interrupt:   Off
// Compare A Match Interrupt: Off
// Compare B Match Interrupt: Off
//
//-- регистр управления A
// 7:4 биты контролируют два выхода OC1A и OC1B
// 3:2 биты не исп.
// 1:0 биты - для вкл. режима ШИМа - записать туда '1'
TCCR1A	= 0xF3;
//-- регистр управления B
// 2:0 биты выбор тактирования для таймера-счетчика TC1
TCCR1B	= 0x01;				// тактирование от "CLK"
//-- счетный регистр
TCNT1H	= 0x00;
TCNT1L	= 0x00;
//-- регистр захвата входа
ICR1H	= 0x00;
ICR1L	= 0x00;
//-- регистр сравнения выхода OC1A (от значения в нем зависит ширина импульса)
OCR1AH	= 0x00;
OCR1AL	= 0x00;
//-- регистр сравнения выхода OC1B
OCR1BH	= 0x00;
OCR1BL	= 0x00;



//-- Timer/Counter 2 инициализация
// Clock source: System Clock
// Clock value:  Timer 2 Stopped
// Mode: Normal top = FFh
// OC2 output: Disconnected
ASSR	= 0x00;
TCCR2	= 0x00;
TCNT2	= 0x00;
OCR2	= 0x00;


//-- External Interrupt(s) инициализация
// INT0: Off
// INT1: Off
MCUCR	= 0x00;


//-- Timer(s)/Counter(s) Interrupt(s) инициализация
TIMSK	= 0x00;


//-- USART инициализация
// Communication Parameters: 8 Data, 1 Stop, No Parity
// USART Receiver: On
// USART Transmitter: On
// USART Mode: Asynchronous
// USART Baud Rate: 9600 (UBRRH+UBRRL)
UCSRA	= 0x00;		// регистр состояния
UCSRB	= 0x98;		// регистр управления
UCSRC	= 0x86;		// регистр формата кадра
//--Clock = 5.12 MHz (32)
UBRRH	= 0x00;		// настройка скорости
UBRRL	= 0x20;		// передачи данных
//--Clock = 8.0 MHz (51)					//my
//UBRRH	= 0x00;		// настройка скорости
//UBRRL	= 0x33;		// передачи данных
//--Clock = 12.0 MHz (77)
//UBRRH	= 0x00;		// настройка скорости
//UBRRL	= 0x4D;		// передачи данных
//--Clock = 16.0 MHz (103)
//UBRRH	= 0x00;		// настройка скорости
//UBRRL	= 0x67;		// передачи данных


//-- Analog Comparator инициализация
// Analog Comparator: Off
// Analog Comparator Input Capture by Timer/Counter 1: Off
ACSR	= 0x80;
SFIOR	= 0x00;


//-- ADC инициализация
// ;ADC Clock frequency: 160,000 KHz (2:0 биты) при 5.12MHz
// ;Only the 8 most significant bits of the AD conversion result are used
// ADC Voltage Reference: AREF pin
//
//-- регистр, задается номер вывода подкл. ко входу АЦП
//ADMUX	= FIRST_ADC_INPUT | (ADC_VREF_TYPE & 0xFF);		// 2 | (0x20 & 0xFF) правое выражение 5-бит=1 (запись результата ацп из ADCH)
ADMUX	= FIRST_ADC_INPUT;					//my запись результата из ADCW (ADCH+ADCL)
//-- регистр контроля и состояния АЦП
//ADCSRA	= 0xCD;							// биты 2:0 clock/32=160KHz частота работы АЦП при 5.12 MHz
ADCSRA	= 0xCE;							//my  биты 2:0 clock/64=125KHz частота работы АЦП при 8.0 MHz
ADCSRA	|= 0x40;						// работа с 6-м битом



// Global enable interrupts
#asm("sei")



////////////////////////////////////////////////////////////////
// Вызываем функции для настройки рабочего состояния станции
////////////////////////////////////////////////////////////////
Set_Fan(2, On);							// 2-й вентилятор = On
//Set_Fan(1, On);							// 1-й вентилятор = On
//Set_Fan(3, Off);							// 3-й вентилятор = Off
Set_PWM_Value(1, 1023);						// 1-й нагреватель = Off (номер, значение шим)
Set_PWM_Value(2, 1023);						// 2-й нагреватель = Off (номер, значение шим)


////////////////////////////////////////////////////////////////
// Опрос USART-а
////////////////////////////////////////////////////////////////
while (1) {
   UART();
}

}// main
