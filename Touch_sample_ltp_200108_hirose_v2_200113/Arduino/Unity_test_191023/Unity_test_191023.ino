#define SPEED1 250  // スライダを伸ばす場合（仮想物体を上向きになぞる場合）
#define SPEED2 5  // スライダを縮める場合
#define SPEED3 180  // スライダを押しとどめる場合（仮想物体を下向きになぞっている際にめり込んだ場合）
#define JITTER 10    // スライダが正しい位置に移動したかを判定する閾値
#define MAX_SPEED 255
#define MIN_SPEED 80 //動かす速度の最低値 80
#define MAX_LEN 1023 //sliderの長さの最大値

int slide;        // 受信したデータ（スライダを移動させる位置）
int upSpeed, downSpeed;  // 指定したモータの回転速度（現在未使用）
int diff;         // 指定されたスライダの位置と現在の位置の差
//int preSlide;
int vr;
int last_vr;
float speed;

//文字列分割関数
int split(String data, char delimiter, String *dst){
    int index = 0;
    int arraySize = (sizeof(data)/sizeof((data)[0]));  
    int datalength = data.length();
    for (int i = 0; i < datalength; i++) {
        char tmp = data.charAt(i);
        if ( tmp == delimiter ) {
            index++;
            if ( index > (arraySize - 1)) return -1;
        }
        else dst[index] += tmp;
    }
    return (index + 1);
}

void speed_change(int speedFlag){
  if(speedFlag == -1){
    digitalWrite(9, HIGH);
  }else if(speedFlag == -2){
    downSpeed = SPEED1;   // yama 181215 本来ならばここでスピードを変更すべきだががたつくため変更せず
  }else if(speedFlag == -3){
    downSpeed = SPEED2;
  }
}

void setup() {
  Serial.begin(38400);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // PWMによるスピード制御 (0-255)

  digitalWrite(12, HIGH);
  digitalWrite(9, HIGH);

  downSpeed = SPEED2;
}

void loop() {  
  if ( Serial.available() ) {
    //key_move(str);
    String recv_data[2] = {"\0"}; 
    String str = Serial.readStringUntil(';');
    int index = split(str, ',', recv_data);
    slide = recv_data[0].toInt();
    int max_length_flag = recv_data[1].toInt();
    //slide = str.toInt();
    //Serial.println("1");
    //Serial.print("flag:");
    //Serial.println(max_length_flag);
    
    if(slide >= 0){
      slide_device(max_length_flag);
    }else{
      speed_change(slide);
    }
  }

  //int v = analogRead(A2);
  //Serial.println(v);
  //Serial.println(speed_level);
  //Serial.println(1);
}

void slide_device(int max_length_flag){
  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  
  if(vr > slide){     // モーターA: 正転（伸びる場合）
    if(max_length_flag == 0){
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
      }
    }else if(max_length_flag == 1){
    
      digitalWrite(12, HIGH);
      digitalWrite(9, LOW);
    
    /*
    if(preSlide < slide){
      upSpeed = SPEED3;
    }
    else{
      upSpeed = SPEED1;
    }
  */
      upSpeed = SPEED1;
  
      analogWrite(3, upSpeed);
    
      while(vr > slide){    // 受信したデータよりスライダの位置が大きい場合
        vr = analogRead(A2);
        diff = vr - slide;
        if(-JITTER < diff && diff < JITTER){  // 一定の閾値以内であれば指定の位置まで移動したとみなす
          break;
        }
        else if(diff < -(JITTER + 40)){   // めり込みすぎた場合に押し返す力を最大限にする
          //upSpeed = 255;
          analogWrite(3, 255);
        }
        if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
          String str = Serial.readStringUntil(';');
          //preSlide = slide;
         slide = str.toInt();

          if(slide < 0){  // デバイスがめり込んだ状態で仮想物体から離れた場合
            break;
          }
        }
      }
    }
    delay(1);  
    digitalWrite(9, HIGH);
  }else if(vr < slide){     // モーターA: 逆転（縮む場合）
    if(max_length_flag == 0){
      digitalWrite(12, LOW);
      digitalWrite(9, LOW);
  
      //downSpeed = SPEED2;
      analogWrite(3, downSpeed);
     
      while(vr < slide){    // 受信したデータよりスライダの位置が小さい場合
        vr = analogRead(A2);
        int diff = vr - slide;
        if(-JITTER < diff && diff < JITTER){  // 一定の閾値以内であれば指定の位置まで移動したとみなす
          break;
        }
        if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
          String str = Serial.readStringUntil(';');
          // preSlide = slide;
          slide = str.toInt();
  
          if(slide < 0){  // デバイスがめり込んだ状態で仮想物体から離れた場合
            break;
          }
        }
      }
    }/*else if(max_length_flag == 1){
      digitalWrite(12, LOW);
      digitalWrite(9, LOW);

      downSpeed = SPEED2;
      analogWrite(3, downSpeed);
   
      while(vr < slide){    // 受信したデータよりスライダの位置が小さい場合
        vr = analogRead(A2);
        int diff = vr - slide;
        if(-JITTER < diff && diff < JITTER){  // 一定の閾値以内であれば指定の位置まで移動したとみなす
          break;
        }
        if ( Serial.available() ) {   // 上記で制止しなかったとしても，伸縮距離が変更されているかもしれないので再度データを受信
          String str = Serial.readStringUntil(';');
          //preSlide = slide;
          slide = str.toInt();

          if(slide < 0){  // デバイスがめり込んだ状態で仮想物体から離れた場合
            break;
          }
        }
      }
    }
      /*
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
      /*
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
      */
       
      delay(1);
      digitalWrite(9, HIGH);
    }
    digitalWrite(9, HIGH); 
  }
 // preSlide = slide;
//}



//void key_move(char mode){
//  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
//  
//  switch (mode) {
//    case '0' : 
//      digitalWrite(12, HIGH);
//      digitalWrite(9, LOW);
//      while(vr > 0){
//        analogWrite(3, SPEED1);
//        vr = analogRead(A2);
//      }
//      digitalWrite(12, HIGH);
//      digitalWrite(9, HIGH); 
//      analogWrite(3, SPEED1);
//      break;
//    case '1' : 
//      digitalWrite(12, LOW);
//      digitalWrite(9, LOW); 
//      while(vr < 512){
//        analogWrite(3, SPEED1);
//        vr = analogRead(A2);
//      }
//      digitalWrite(12, HIGH);
//      digitalWrite(9, HIGH);
//      analogWrite(3, SPEED1); 
//      break;
//    case '2' :
//      digitalWrite(12, HIGH);
//      digitalWrite(9, HIGH); 
//      break;
//  }
//
//  delay(10);
//}
