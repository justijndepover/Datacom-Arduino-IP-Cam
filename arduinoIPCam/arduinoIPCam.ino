#include <SimpleTimer.h>
#include <LiquidCrystal.h>
LiquidCrystal lcd(6,7,5,4,3,2);
SimpleTimer timer;
int button11 = 0;
int sensorPinTemp = A5;
int sensorPinLV = A4;
bool toggle = false;
int tmrWelkomId;
int tmrBelId;
int tmrWachtOpAntwoordId;
bool welkomTempToggle=false;
double tempmeter = 0.0;
double lvmeter=0.0;

void setup() {
  pinMode(11, INPUT_PULLUP);
  Serial.begin(9600);
  tmrWelkomId=timer.setInterval(3000,timerWelkomEvent);
  timer.enable(tmrWelkomId);
  tmrBelId=timer.setInterval(10000,timerBelEvent);
  timer.disable(tmrBelId);
  tmrWachtOpAntwoordId = timer.setInterval(10000,timerWachtOpAntwoordEvent);
  timer.disable(tmrWachtOpAntwoordId);
  lcd.begin(16,2);
  lcd.setCursor(0,0);
  lcd.clear();  
}

void loop() { 
  timer.run();
  leesSerialPort(); 
  checkDeBel();
}

void timerWelkomEvent(){
  welkomTempToggle = !welkomTempToggle;
  if(welkomTempToggle==true){
    lcd.clear();
    lcd.print("Welkom!");
  }
  else{  
    lcd.clear();    
    meetTemp();
    meetLV();
    lcd.setCursor(0,0);
    lcd.print("T:"+String(tempmeter)+"C");
    lcd.setCursor(0,1);
    lcd.print("LV:"+String(lvmeter)+"%");
  }
}

void timerBelEvent(){
  timer.enable(tmrWelkomId);
  timer.disable(tmrBelId);
}

void timerWachtOpAntwoordEvent(){
  timer.enable(tmrWelkomId);
  timer.disable(tmrBelId);
}


void meetTemp(){
  double buffertempmeter=(((analogRead(sensorPinTemp)*4.9) / 1024.0)-0.5)*100;  
  if(tempmeter!=buffertempmeter){
    tempmeter=buffertempmeter;
  }
}

void meetLV(){
  double bufferlv = ((double)analogRead(sensorPinLV))/1023.0*100;
  if(lvmeter!=bufferlv){  
    lvmeter=bufferlv;
  }
}

void checkDeBel(){
  button11 = digitalRead(11);
  if(button11 == LOW){
    if(toggle==false)
    {
      lcd.clear();
      lcd.print("Ring! Ring!");
      // de bel gaat
      Serial.println("1");
      timer.disable(tmrWelkomId);
      timer.enable(tmrWachtOpAntwoordId);    
    }
    toggle=true;
  }
  else{
    toggle=false;
  }
}

void leesSerialPort(){
  if(Serial.available()>0){
    char incomingChar = Serial.read();
    if(incomingChar=='2'){
      timer.disable(tmrWachtOpAntwoordId);
      timer.disable(tmrWelkomId);  
      lcd.clear();
      lcd.print("U mag binnen");
      lcd.setCursor(0,1);
      lcd.print("komen");
      timer.enable(tmrBelId);
    }else if(incomingChar=='3'){
      timer.disable(tmrWachtOpAntwoordId);
      timer.disable(tmrWelkomId);    
      lcd.clear();
      lcd.print("U mag niet");
      lcd.setCursor(0,1);
      lcd.print("binnen komen");
      timer.enable(tmrBelId);
    }
  } 
}
