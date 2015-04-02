#include <LiquidCrystal.h>
LiquidCrystal lcd(6,7,5,4,3,2);
int button11 = 0;
int sensorPin = A5;
bool toggle = false;
bool deBel = false;

void setup() {
  pinMode(11, INPUT_PULLUP);
  Serial.begin(9600);
  
  lcd.begin(16,2);
  lcd.setCursor(0,0);
  lcd.clear();
  
  lcd.print("Welkom!");
}

void loop() {  
  checkDeBel();
  if(deBel==false){
    meetTemp();
  }
}

double tempmeter = 0;
void meetTemp(){
  double buffertempmeter=(((analogRead(sensorPin)*4.9) / 1024.0)-0.5)*100;  
  if(tempmeter!=buffertempmeter){
    tempmeter=buffertempmeter;
    lcd.clear();
    lcd.print(String(buffertempmeter)+"C");
  }
}

void checkDeBel(){
  button11 = digitalRead(11);
  
  if(button11 == LOW){
    if(toggle==false)
    {
      lcd.clear();
      deBel=true;
      // de bel gaat
      Serial.println("1");
    }
    toggle=true;
  }
  else{
    if(toggle==true){
      lcd.clear();
    }
    toggle=false;
  }
}
