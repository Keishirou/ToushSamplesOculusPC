#define SPEED1 255  // スライダを縮める場合
#define SPEED2 170  
#define MAX_SPEED 255
#define MIN_SPEED 160 //動かす速度の最低値
#define MAX_LEN 1023 //sliderの長さの最大値
#define JITTER 20 // スライダが正しい位置に移動したかを判定する閾値

int slide;        // 受信したデータ（スライダを移動させる位置）
int speed_level;  // 指定したモータの回転速度（現在未使用）
int diff;         // 指定されたスライダの位置と現在の位置の差
int vr;
int last_vr;
float speed;

void setup() {
  Serial.begin(38400);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // PWMによるスピード制御 (0-255)

  digitalWrite(12, HIGH);
  digitalWrite(9, HIGH);
  analogWrite(3, SPEED1);
  
  speed_level = SPEED1;
  vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
}

void loop() {  
  if ( Serial.available() ) {
    //key_move(str);

    String str = Serial.readStringUntil(';');
    slide = str.toInt();

    if(slide >= 0){
      slide_device();
    }else{
      speed_change(slide);
    }
  }

  int v = analogRead(A2);
  Serial.println(v);
}

/*伸縮処理*/
void slide_device(){
   last_vr = vr;
    vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
      
  if(vr > slide){     // モーターA: 正転
     //speed = (MAX_SPEED-MIN_SPEED)*(1.0-(float)(vr - slide)/MAX_LEN); 遠い＝遅い
     speed = (MAX_SPEED-MIN_SPEED)*((float)(vr - slide)/MAX_LEN); //遠い＝早い
    digitalWrite(12, HIGH); //正転
    digitalWrite(9, LOW); //ブレーキOFF
    analogWrite(3, speed+MIN_SPEED);
    
    while(vr > slide){    // 受信したデータよりスライダの位置が大きい場合
      vr = analogRead(A2);
      //speed = (MAX_SPEED-MIN_SPEED)*(1.0-(float)(vr - slide)/MAX_LEN); 遠い＝遅い
     speed = (MAX_SPEED-MIN_SPEED)*((float)(vr - slide)/MAX_LEN); //遠い＝早い
      analogWrite(3, speed+MIN_SPEED);
      diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){  // 一定の閾値以内であれば指定の位置まで移動したとみなす
        //slide = vr;
        //digitalWrite(9, HIGH); //LOW…ブレーキOFF
        break;
      }
      /*割り込み処理で代替しようとしたけどこっちじゃないと即オーバーフローorガタガタ*/
      if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
        String str = Serial.readStringUntil(';');
        //Serial.flush();
        slide = str.toInt();
         if(slide < 0){
          //digitalWrite(9, HIGH);
          break;
        }
        vr = analogRead(A2);
         //speed = (MAX_SPEED-MIN_SPEED)*(1.0-(float)(vr - slide)/MAX_LEN); 遠い＝遅い
     speed = (MAX_SPEED-MIN_SPEED)*((float)(vr - slide)/MAX_LEN); //遠い＝早い
      analogWrite(3, speed+MIN_SPEED);
         Serial.println(vr);
      }
      delay(1);
    }  

    //どっちかのみだとがたつくことを確認
    //どっちもなかったらクラッシュ
    digitalWrite(9, HIGH); //ブレーキON
    //analogWrite(3,  SPEED1);
  }
  else if(vr < slide){     // モーターA: 逆転 
     //speed = (MAX_SPEED-MIN_SPEED)*(1.0-(float)(slide - vr)/MAX_LEN); 遠い＝遅い
     speed = (MAX_SPEED-MIN_SPEED)*((float)(slide - vr)/MAX_LEN); //遠い＝早い
      
      digitalWrite(12, LOW);  //逆転
      digitalWrite(9, LOW); //ブレーキOFF
      analogWrite(3, speed+MIN_SPEED);
   
   while(vr < slide){    // 受信したデータよりスライダの位置が小さい場合
      vr = analogRead(A2);
      //speed = (MAX_SPEED-MIN_SPEED)*(1.0-(float)(slide - vr)/MAX_LEN); 遠い＝遅い
     speed = (MAX_SPEED-MIN_SPEED)*((float)(slide - vr)/MAX_LEN); //遠い＝早い
      analogWrite(3, speed+MIN_SPEED);
      int diff = vr - slide;
      if(-JITTER < diff && diff < JITTER){  // 一定の閾値以内であれば指定の位置まで移動したとみなす
        //digitalWrite(9, HIGH); //ブレーキON
        //slide = vr;
        break;
      }
        /*割り込み処理で代替しようとしたけどこっちじゃないと即オーバーフローorガタガタ*/
      if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
        String str = Serial.readStringUntil(';');
        slide = str.toInt();
         if(slide < 0){
          //digitalWrite(9, HIGH);
          break;
        }
        vr = analogRead(A2);
      //speed = (MAX_SPEED-MIN_SPEED)*(1.0-(float)(slide - vr)/MAX_LEN); 遠い＝遅い
     speed = (MAX_SPEED-MIN_SPEED)*((float)(slide - vr)/MAX_LEN); //遠い＝早い
      analogWrite(3, speed+MIN_SPEED);
         Serial.println(vr);
      }
      delay(1);
    }
    digitalWrite(9, HIGH); //ブレーキON
  }
}

void speed_change(int speedFlag){
  if(speedFlag == -1){
    speed_level = SPEED1;
  }else if(speedFlag == -2){
    speed_level = SPEED2;
  }
  analogWrite(3, speed_level);
}

/*
void key_move(char mode){
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  switch (mode) {
    case '0' : 
      digitalWrite(12, HIGH);
      digitalWrite(9, LOW);
      while(vr > 0){
        analogWrite(3, SPEED1);
        vr = analogRead(A2);
      }
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH); 
      analogWrite(3, SPEED1);
      break;
    case '1' : 
      digitalWrite(12, LOW);
      digitalWrite(9, LOW); 
      while(vr < 512){
        analogWrite(3, SPEED1);
        vr = analogRead(A2);
      }
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH);
      analogWrite(3, SPEED1); 
      break;
    case '2' :
      digitalWrite(12, HIGH);
      digitalWrite(9, HIGH); 
      break;
  }

  delay(10);
}*/
