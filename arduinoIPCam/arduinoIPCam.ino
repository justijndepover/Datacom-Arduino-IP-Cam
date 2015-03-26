#include <LiquidCrystal.h>
LiquidCrystal lcd(6,7,5,4,3,2);
int button11 = 0;
int sensorPin = A2;

void setup() {
  pinMode(11, INPUT_PULLUP);
  Serial.begin(9600);
  
  lcd.begin(16,2);
  lcd.setCursor(0,0);
  lcd.clear();
  
  lcd.print("Welkom!");
}

bool toggle = false;

void loop() {
  int sensorValue = analogRead(sensorPin);
  button11 = digitalRead(11);
  Serial.println(String(sensorValue));
  
  if(button11 == LOW){
    if(toggle==false)
    {
      lcd.clear();
      
      Serial.println("LOW");
    }
    toggle=true;
  }
  else{
    if(toggle==true){
      lcd.clear();
      
      Serial.println("HIGH");
    }
    toggle=false;
  }
}
