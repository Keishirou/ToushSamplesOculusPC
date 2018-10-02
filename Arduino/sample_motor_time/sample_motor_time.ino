#define SPEED 255
unsigned long time_m;

void setup() {
  Serial.begin(9600);
  // モーターAの制御用ピン設定
  pinMode(12, OUTPUT); // 回転方向 (HIGH/LOW)
  pinMode(9, OUTPUT); // ブレーキ (HIGH/LOW)
  pinMode(3, OUTPUT); // A PWMによるスピード制御 (0-255)
}

void loop() {

  int vr = analogRead(A2); //A2にボリューム部の2ピンを接続。1=5V,0=GND
  time_m = millis();
  
  // モーターA: 正転
  digitalWrite(12, HIGH);
  digitalWrite(9, LOW);

  while(vr > 0){ //一番端っこ(抵抗値がMAX)に行くまで正転
    vr = analogRead(A2);
    analogWrite(3, SPEED);
    Serial.print("vr =");
    Serial.println(vr);
    Serial.print(" ");
    Serial.print("time =");
    Serial.println(time_m);
  }
  digitalWrite(9, HIGH); //端っこに行ったらブレーキ
  
  // 2秒間上記設定で回転
  delay(10);
}
