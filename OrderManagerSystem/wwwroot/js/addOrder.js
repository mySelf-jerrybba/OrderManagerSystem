
document.addEventListener("DOMContentLoaded", init);


// 初始化程式
async function init() {
    //假設日期條件
    const year = 2026;
    const month = 6;
    console.log("執行初始化程式init");
    console.log("去資料庫撈一整個月的排程資料");
    var chartInfo = await preMakeChart(year, month);//await避免時間差拿到空的object
    // console.log("test");
    // console.log("chartInfo:", chartInfo);
    // console.log("type:", typeof chartInfo);
    // console.log("keys:", Object.keys(chartInfo));

    console.log("繪製圖表");
    await showScheduledChart(year, month,chartInfo);
    console.log("初始化程式init執行結束");
}

function formatDateId(date) {
    //把日期變成mysql看得懂的格式
    //例如："20260501"
    console.log("將年月日轉換成mysql的dateTime格式");
    console.log("執行formatDateId");
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    console.log("formatDateId執行結束");
    return `${y}${m}${d}`;
}

function formatDate(str) {
    const parts = str.split(" ")[0].split("/");

    const month = Number(parts[1]);
    const day = Number(parts[2]);

    return `${month}/${day}`;
}

//預防fetch → DB query → group → return整整30次造成API附載過重
//直接取整個月的資料出來在繪製圖表
async function preMakeChart(year, month) {
    console.log("執行preMakeChart"); 
 
    //月初
    const monthStart = new Date(year, month - 1, 1);
    const monthStartId=formatDateId(monthStart);//ex："20250601"
    //月尾
    const monthEnd = new Date(year, month, 0);
    const monthEndId = formatDateId(monthEnd);//ex："20250630"

    console.log("月初：" + monthStartId);
    console.log("月尾：" + monthEndId);
    
    console.log("處理" + year +"年" + month + "月排程資訊");
    
    //`文字 ${變數}`
    //若date=date會送出文字而不是變數
    console.log("step1.索取資料");
    const request = await fetch(`/getScheduled?monthStartId=${monthStartId}&monthEndId=${monthEndId}`);

    if (!request.ok) {
        console.error("API錯誤", request.status);
        return {};
    }

    console.log("step2.取得資料");
    //chartData是一個陣列，包含dto格式的資料
    //排成日期(bucketDate)，工站名稱(workStation)，訂單編號(orderId)，預估工時(workingHours)，是否跨日加工(isWorkingOverNight)
    const chartData = await request.json();
    
    //把資料按日期整理方便取用
    //例如：
    // [
    //     {
    //         bucketDate: "20260601",
    //         orderId: "MO001"
    //     },
    //     {
    //         bucketDate: "20260601",
    //         orderId: "MO002"
    //     }
    // ];
    //轉變成如下格式：
    // {
    //     "20260601": {
    //          remainTime: 3,
    //          orders:
    //          [
    //              {
    //                  bucketDate: "20260601",
    //                  orderId: "MO001"
    //              },
    //              {
    //                  bucketDate: "20260601",
    //                  orderId: "MO002"
    //              }
    //          ]    
    //      }
    // }
    console.log("step3.將資料按日期分類");
    const grouped = {};
    chartData.forEach(item => {
        if (!grouped[item.bucketDate]) {
            grouped[item.bucketDate] = {
                remainTime: item.remainTime,
                orders: []
            };
        }
        grouped[item.bucketDate].orders.push({
            orderId: item.orderId,
            workingHours: item.workingHours,
            isWorkingOverNight: item.isWorkingOverNight
        });
    });

    console.log("preMakeChart執行結束");
    return grouped;
}

function showScheduledChart(year, month, chartInfo) {
    console.log("執行showScheduledChart");

    const scheduledChart = document.getElementById("scheduledChart");
    // 先清空舊內容
    scheduledChart.innerHTML = "";
    //兩個時間都是date物件
    //例如：Mon Jun 01 2026 00:00:00 GMT+0800 (台北標準時間)
    //月初
    const monthStart = new Date(year, month - 1, 1);
    //月尾
    const monthEnd = new Date(year, month, 0); 

    console.log("月初：" + monthStart);
    console.log("月尾：" + monthEnd);
  
    // 在JS:
    // 星期有數字編號，例如：日=0 ，一=1 ...
    let startWeekDay = monthStart.getDay();
    console.log("開始繪製圖表");
    console.log("繪製當月空缺的天數");
    // 前面補空格
    for (let i = 1; i < startWeekDay; i++) {
        const emptyDiv =document.createElement("div");
            
        emptyDiv.className ="day empty";//css=.empty
            
        scheduledChart.appendChild(emptyDiv);
    } 

    console.log("繪製當月實際天數");
    // 該月總天數
    const daysInMonth = new Date(year, month, 0).getDate(); 

    //chartInfo是打包好的一整個月的排程資料
    //逐一拿出 object（物件）裡的「key（鍵）」來跑
    console.log("準備把處理好的資料放到html的圖表");

    for (const bucketDate in chartInfo) { 
        let html = "";
        //放入內容清單
        //第一層是日期，往下遍歷
        //建立div元素
        const div = document.createElement("div");
        //賦予class
        div.className = "day";
        // console.log("bucketDate=<" + bucketDate + ">");bucketDate是string
        const today = formatDate(bucketDate);
        html +=
            `
                 <div>
                    日期：${today}<br>
                    時間桶餘額：${chartInfo[bucketDate].remainTime}
                </div>
            `;
        chartInfo[bucketDate].orders.forEach(order => {
            // console.log(order.orderId);
            // console.log(order.workingHours);
            // console.log(order.isWorkingOverNight);
            // console.log("------------------------");
            html+=
                `<div>
                    <p>訂單編號：${order.orderId}</p>
                    <p>預估工時：${order.workingHours}</p>
                    <p>是否跨日加工：${order.isWorkingOverNight}</p>
                </div>`;
        });
        //建立div欄位
        div.innerHTML = html;
        scheduledChart.appendChild(div);
    }
    console.log("showScheduledChart執行結束");
}

///新增訂單進入排程///
async function insertNewOrder() {
    //申請想要新增的排程
    //把html填入欄位的值打包成物件
    console.log("執行insertNewOrder");
    const userInputPack = {
        orderId: document.getElementById("orderId").value,//訂單編號
        applyDate: document.getElementById("applyDate").value,//建立日期
        deadline: document.getElementById("deadline").value,//交貨日期
        estimateStartDate: document.getElementById("estimateStartDate").value,//預計加工日期
        estimateEndDate: document.getElementById("estimateEndDate").value,//預計完成日期
        amount: document.getElementById("amount").value,//交貨數量
        location: document.getElementById("location").value,//送貨地點
        mainReceiver: document.getElementById("mainReceiver").value,//收貨負責人
        estimateWorkingTime: document.getElementById("estimateWorkingTime").value//預估工時
    };
    console.log("userInputPack=", userInputPack);

    //先把物件傳給controller讓service做排程可行性邏輯評估
    const response =await fetch('/applyAddOrder', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(userInputPack)
    });

    //預防request出現異常
    if (!response.ok) {
        console.error("API錯誤", response.status);
        return {};
    }

    
    const applyResult = await response.json();
    if (applyResult == "true") {
        //input->js->controller
        //service做排程可行性檢查
        //通過後service再接續執行insert程序
        console.log("排程新增成功");
        alert("排程新增成功");
        console.log("更新所顯示排成圖表資訊");
        //假設日期條件
        const year = 2026;
        const month = 6;
        var chartInfo = await preMakeChart(year, month);
        console.log("檢查畫面更新時拿到的資料，chartInfo=", chartInfo);
        await showScheduledChart(year, month, chartInfo);
        console.log("所顯示排成圖表資訊更新結束");
    }
    else {
        //回傳排程可行性評估失敗或排程新增失敗的錯誤訊息
        console.log(applyResult);
        alert(applyResult);
    }
    console.log("insertNewOrder執行結束");
}