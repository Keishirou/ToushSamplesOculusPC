#define SPEED 150

int slide;
int moving;
int flag;

void setup() {
  Serial.begin(9600);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // PWMによるスピード制御 (0-255)

  digitalWrite(12, HIGH);
  digitalWrite(9, HIGH);
  analogWrite(3, SPEED);

  moving = 0;
  flag = 0;
}

void loop() {
  // put your main code here, to run repeatedly:
  //int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  if ( Serial.available() ) {
    /*
    char str = Serial.read();
    slide = (int)str - (int)'0';

    //int slide = Serial.read();

    slide = 1024 - slide * 8;
*/
    //key_move(str);

    String str = Serial.readStringUntil(';');
    slide = str.toInt();
    //Serial.println(num);

    slide_device();
  }
  /*
  if(vr > slide){
    // モーターA: 正転
    digitalWrite(12, HIGH);
    digitalWrite(9, LOW);
    while(vr > slide){
      analogWrite(3, SPEED);
      vr = analogRead(A2);
    }
    digitalWrite(12, HIGH);
    digitalWrite(9, HIGH); 
  }else if(vr < slide){
    // モーターA: 逆転
    digitalWrite(12, LOW);
    digitalWrite(9, LOW);
   while(vr < slide){
      analogWrite(3, SPEED);
      vr = analogRead(A2);
    }
    digitalWrite(12, HIGH);
    digitalWrite(9, HIGH);
  }else{
    digitalWrite(12, HIGH);
    digitalWrite(9, HIGH);
  }
  analogWrite(3, SPEED);
  delay(10);
  */
}

void slide_device(){
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  if(vr > slide){
    // モーターA: 正転
    digitalWrite(12, HIGH);
    digitalWrite(9, LOW);
    while(vr > slide){
      analogWrite(3, SPEED);
      vr = analogRead(A2);
    }
    digitalWrite(12, HIGH);
    digitalWrite(9, HIGH); 
  }else if(vr < slide){
    // モーターA: 逆転
    digitalWrite(12, LOW);
    digitalWrite(9, LOW);
   while(vr < slide){
      analogWrite(3, SPEED);
      vr = analogRead(A2);
    }
    digitalWrite(12, HIGH);
    digitalWrite(9, HIGH);
  }else{
    digitalWrite(12, HIGH);
    digitalWrite(9, HIGH);
  }
  analogWrite(3, SPEED);
  delay(10);
}

void key_move(char mode){
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  switch (mode) {
    case '0' : 
      digitalWrite(12, HIGH);
      digitalWrite(9, LOW);
      while(vr > 0){
        analogWrite(3, SPEED);
        vr = analogRead(A2);
      }
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH); 
      analogWrite(3, SPEED);
      break;
    case '1' : 
      digitalWrite(12, LOW);
      digitalWrite(9, LOW); 
      while(vr < 512){
        analogWrite(3, SPEED);
        vr = analogRead(A2);
      }
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH);
      analogWrite(3, SPEED); 
      break;
    case '2' :
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH); 
      break;
  }

  delay(10);
}

