clear all
pkg load signal
pkg load statistics

n = 1:1000;

t =mod(mod((n*2147483629),104651).*n , 3391)/3391;
t2 =mod(mod((n*2147483629),104651) , 3391)/3391;
t3 =mod((n*2147483629) , 3391)/3391;
t4 = mod((n*2147483629),104651)/104651;
t5 = mod(mod(mod((n*2147483629),104651).*n , 3391).*n,997)/997;
r = random("unif", 0,1,[1,1000]);

T = real(fft(t));
T2 = real(fft(t2));
T3 = real(fft(t3));
T4 = real(fft(t4));
T5 = real(fft(t5));
R = real(fft(r));

figure
hold on
plot(T(2:1000))
plot(R(2:1000), 'g')
'plot(T2(2:1000),'r')
'plot(T3(2:1000),'c')
'plot(T4(2:1000),'m')
plot(T5(2:1000),'y')

figure
hold on
plot(t(2:10000))
plot(r(2:10000), 'g')
plot(t2(2:10000),'r')
plot(t3(2:10000),'c')
plot(t4(2:10000),'m')

