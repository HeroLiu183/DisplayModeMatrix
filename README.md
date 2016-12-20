# DisplayModeMatrix

*�\Ū��L�y������: [English](README.en-us.md), [���餤��](README.md).*

DisplayModeMatrix �Ψ��X�i ASP.NET MVC Display Modes �w�]�w�]����@���סA���ѥi���i���զX�ʡC  

������� Android �ұҵo, �ԥi�Ѧ� [How Android Finds the Best-matching Resource](https://developer.android.com/guide/topics/resources/providing-resources.html#BestMatch)  

���F�h���ת� Display Modes�A�A�i�H�G

- ���ѫܴΪ����� A/B testing ����
- �b�h�������ε{�����D�`�e�����覡���ѫȻs�Ƴ���榡
- �h�� Display Modes �P�ɤu�@

## �򥻷Q�k

�}�o�̹B�� ASP.NET MVC Display Modes �N View �b���P���ҤU�����h�Ӫ����C 
 
�`�����Ҥl�O�Ϥ��X�ୱ���P��ʪ��� View�C  

�����ε{������ View ���e�ڭ̧Ʊ�ھڱ�����ܥH�U�A�X�� View�C

```
Index.cshtml  
Index.Mobile.cshtml  
```

�ୱ������ Index.cshtml�A��ʪ������� Index.Mobile.cshtml�C

�o�ݭn�H�зǪ� Display Modes ���U�{�ǡA�ϱo ASP.NET MVC ����i�� View �����Ҥ����C

```
DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Mobile")
{
    ContextCondition = (context => context.IsMobile())    
});
```

�H�W�O²�n�� Display Modes ����ԭz�A�۷�²����ΡC

�p�W�A�]�� Display Modes �w�]����@���ת��]�p�A���ɧڭ̻ݭn��[�u�ʪ� Display Modes �պA�覡�A�_�h Display Modes �պA�覡�|�ܱo�D�`���H���g�κ��@�C
���ǸѨM��שγ\�i�ѹ�@ IDisplayMode �����ӹF���A����@�_�ӵy�L�����C

�ڪ��Q�k�O�N suffix ���ܬ��h�q�զX�Ӧ��A�åH "-" �s������_�C

```
Index.{Devices}-{Preview}.cshtml
```

�C�@�ӳ����֦��W�ߪ� *�Ÿ�* �H�� *�B�⦡ (expression)* �c��

- `{Device}` �m���� "Mobile", �p�G HttpContext.Current.IsMobile() ����
- `{Preview}` �m���� "Preview", �p�G�S�w�� cookie �s�b��ШD���Y�W  

����@�ӳ����i����ܩʦs�b�A�p�G�ӳ����S���������ҡA���N�d�աC

�S���N�q���s���Ÿ����|�����c�� suffix ������(�Y�B���B���ƪ�)�C

�M��H Builder pattern ��U�p��X Display Modes ���զX�ʡC

## �p��ϥ�

### ���]�T�եi��ܩʪ����סA�C�@�Ӻ��ץH�Υi�઺�Ȧp�U��

|           ����          |                       �i���                       |
|-------------------------|---------------------------------------------------|
| **Device** (�i��)       | *Mobile*, *Tablet*, *Default* (�ŭ�)               |
| **Theme** (�i��)       | *Dark*, *Default* (�ŭ�)                            |
| **Preview** (�i��)     | *Preview*, *No Preview* (�ŭ�)                      |

### �w�����ͪ� suffix �զX�H�Υ��T����

- Mobile-Dark-Preview
- Tablet-Dark-Preview
- Mobile-Dark
- Tablet-Dark
- Mobile-Preview
- Tablet-Preview
- Dark-Preview
- Mobile
- Tablet
- Dark
- Preview

�o�� suffix �N���Ω�з� Display Modes ���պA���.

### Views �����c

���F�h���ת��i��ʡA�{�b�A�i�H��[���u�ʪ� Display Modes ��´ View�C

![Views structure](screenshot/views-structure.png)

### �ϥ� DisplayModeMatrixBuilder �إߤ@�t�C�� Display Modes

```csharp
var builder = new DisplayModeMatrixBuilder();

var matrix = builder
                .AddOptionalFactor("Device", l => l.Evidence("Mobile", x => IsMobile(x)).Evidence("Tablet", x => IsTablet(x)))
                .AddOptionalFactor("Theme", l => l.Evidence("Dark", x => CurrentTheme(x) == "dark"))
                .AddOptionalFactor("Preview", l => l.Evidence("Preview", x => IsPreview(x)))
                .Build();
```

builder.Build() �i�ͦ��@�� `IEnumerable<DisplayModeProfile>` ���X��Ψӵ��U Display Modes�C 

���U�覡�p�U�A���㪺�d�ҽаѦ� DisplayModeMatrix.Web �M�ת� [~/App_Start/DisplayModeConfig.cs](DisplayModeMatrix.Web/App_Start/DisplayModeConfig.cs)

```csharp
foreach (var profile in matrix)
{
    instance.Modes.Add(new DefaultDisplayMode(profile.Name)
    {
        ContextCondition = x => profile.ContextCondition(x)
    });
}
```

### �į����

�ϥ� [SuperBenchmarker](https://github.com/aliostad/SuperBenchmarker) (-n 1000 -c 10)

|                     | �B�� DisplayModeMatrix         |    �������ϥ� Display Modes    |
|---------------------|--------------------------------|-------------------------------|
| TPS                 | 133.2 (requests/second)        | 135.3 (requests/second)       |
| Max                 | 6018.8998ms                    | 6160.7143ms                   |
| Min                 | 2.4041ms                       | 2.3731ms                      |
| Avg                 | 67.8596257ms                   | 67.777162ms                   |
