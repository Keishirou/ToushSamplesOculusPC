#define SPEED 40
#define MAX_SPEED 255
#define MAX_LEN 1023.0
int vr;
void setup() {
  Serial.begin(9600);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // A PWMによるスピード制御 (0-255)
  vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
}

void loop() {
  int last_vr = vr;
  vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
    
  // モーターA: 正転
  digitalWrite(12, HIGH);
  digitalWrite(9, LOW);

  while(vr > 0){ //一番端っこ(抵抗値がMAX)に行くまで正転
    last_vr = vr;
    vr = analogRead(A2);
    float speed = (MAX_SPEED-SPEED)*((float)(vr - last_vr)/MAX_LEN);
    analogWrite(3, speed+SPEED);
    Serial.println(vr - last_vr);
  }
  digitalWrite(9, HIGH); //端っこに行ったらブレーキ
  
  // 2秒間上記設定で回転
  delay(2000);
  
  // モーターA: 逆転
  digitalWrite(12, LOW);
  digitalWrite(9, LOW);
  
  while(vr <1023){ //一番端っこ(抵抗値が0側。電圧が高くなるほう)に行くまで逆転
    last_vr = vr;
    vr = analogRead(A2);
     float speed = (MAX_SPEED-SPEED)*((float)(last_vr - vr)/MAX_LEN);
    analogWrite(3, speed+SPEED);
    Serial.println(last_vr - vr);
  }
  digitalWrite(9, HIGH); //端っこに行ったらブレーキ
  
  // 2秒間上記設定で回転  
  delay(2000);
}
